using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Contracts.Services;
using CoursePlatform.Application.Features.Auth.Commands.ResendOtp;
using CoursePlatform.Application.Features.Auth.DTOs;
using CoursePlatform.Application.Features.Auth.Events;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMessagePublisher _publisher;
    private readonly IGoogleAuthService _googleAuth;
    private readonly AppDbContext _context;

    private readonly IOtpService _otpService;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IMessagePublisher publisher,
        IGoogleAuthService googleAuth,
        IOtpService otpService,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _publisher = publisher;
        _googleAuth = googleAuth;
        _otpService = otpService;
        _context = context;
    }

    public async Task RegisterAsync(
      string firstName, string lastName,
      string email, string password,
      string role, CancellationToken ct = default)
    {
        // 1. التحقق من عدم تكرار الإيميل
        var exists = await _userManager.FindByEmailAsync(email);
        if (exists is not null)
            throw new ConflictException($"Email '{email}' is already registered.");

        // 2. إنشاء الـ user
        var user = new AppUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = await GenerateUniqueUsernameAsync(firstName, lastName),
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new BadRequestException(
                string.Join(", ", createResult.Errors.Select(e => e.Description)));

        // 3. تعيين الـ Role
        await _userManager.AddToRoleAsync(user, role);

        // 4. توليد OTP وحفظه
        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, ct);

        // 5. إرسال OTP عبر RabbitMQ
        await _publisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            OtpCode = otpCode
        }, "user.registered", ct);
    }

    public async Task<AuthResponseDto> LoginAsync(
        string email, string password,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (user.IsDeleted)
            throw new UnauthorizedException(
                "This account has been deactivated.");

        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new UnauthorizedException(
                "Please confirm your email before logging in.");

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException(
                "Account is temporarily locked. Try again later.");

        var result = await _signInManager
            .CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!result.Succeeded)
            throw new UnauthorizedException("Invalid email or password.");

        var accessToken = await _tokenService.CreateAccessTokenAsync(user);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return MapToDto(user, accessToken, refreshToken.Token, roles);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string accessToken, string refreshToken,
        CancellationToken ct = default)
    {
        var userId = _tokenService.GetUserIdFromExpiredToken(accessToken)
            ?? throw new UnauthorizedException("Invalid access token.");

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException("User not found.");

        var stored = await _tokenService
            .GetActiveRefreshTokenAsync(user.Id, refreshToken, ct)
            ?? throw new UnauthorizedException(
                "Invalid or expired refresh token.");

        await _tokenService.RevokeRefreshTokenAsync(stored, ct);

        var newAccess = await _tokenService.CreateAccessTokenAsync(user);
        var newRefresh = await _tokenService.CreateRefreshTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return MapToDto(user, newAccess, newRefresh.Token, roles);
    }

    public async Task VerifyEmailAsync(
      string email, string code,
      CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new NotFoundException("User", email);

        if (user.EmailConfirmed)
            throw new BadRequestException("Email is already verified.");

        var isValid = await _otpService.ValidateOtpAsync(user.Id, code, ct);
        if (!isValid)
            throw new BadRequestException("Invalid or expired OTP code.");

        // تأكيد الإيميل يدوياً
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    // في Infrastructure/Services/AuthService.cs

    public async Task ForgotPasswordAsync(
        string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // لا نكشف معلومات — security best practice
        if (user is null || !user.EmailConfirmed || user.IsDeleted)
            return;

        // OTP مش token
        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, ct);

        await _publisher.PublishAsync(new PasswordResetRequestedEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            OtpCode = otpCode       // ← OTP
        }, "password.reset.requested", ct);
    }

    public async Task ResetPasswordAsync(
        string email, string otpCode,
        string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new NotFoundException("User", email);

        // 1. التحقق من الـ OTP
        var isValid = await _otpService.ValidateOtpAsync(user.Id, otpCode, ct);
        if (!isValid)
            throw new BadRequestException("Invalid or expired OTP code.");

        // 2. reset الـ password بدون token
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(
                             user, resetToken, newPassword);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }
    public async Task ChangePasswordAsync(
        Guid userId, string currentPassword,
        string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var result = await _userManager
            .ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }



    public async Task<AuthResponseDto> GoogleLoginAsync(
    string idToken, CancellationToken ct = default)
    {
        // verify token and get user info from Google
        var googleUser = await _googleAuth.VerifyIdTokenAsync(idToken, ct)
            ?? throw new UnauthorizedException("Invalid Google token.");

        // check if email is present and verified by Google
        if (string.IsNullOrEmpty(googleUser.Email))
            throw new BadRequestException(
                "Google account does not have a verified email. " +
                "Please use an account with a verified email address.");

        // get or create user
        var user = await _userManager.FindByEmailAsync(googleUser.Email);

        if (user is null)
        {
            // Split FullName for FirstName and LastName
            var nameParts = googleUser.FullName?.Split(' ', 2)
                            ?? ["", ""];
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            user = new AppUser
            {
                Email = googleUser.Email,
                UserName = googleUser.Email,
                FirstName = googleUser.FirstName,   
                LastName = googleUser.LastName,
                ProfilePictureUrl = googleUser.PictureUrl,
                EmailConfirmed = true,   // ← Google verified الـ email
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(
                    string.Join(", ",
                        result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Student");
        }
        else
        {
            var needsUpdate = false;

            if (string.IsNullOrEmpty(user.FirstName) &&
                !string.IsNullOrEmpty(googleUser.FirstName))
            {
                user.FirstName = googleUser.FirstName;
                needsUpdate = true;
            }

            if (string.IsNullOrEmpty(user.LastName) &&
                !string.IsNullOrEmpty(googleUser.LastName))
            {
                user.LastName = googleUser.LastName;
                needsUpdate = true;
            }

            if (!string.IsNullOrEmpty(googleUser.PictureUrl) &&
                user.ProfilePictureUrl != googleUser.PictureUrl)
            {
                user.ProfilePictureUrl = googleUser.PictureUrl;
                needsUpdate = true;
            }

            if (needsUpdate)
                await _userManager.UpdateAsync(user);
        }
        // check IsDeleted and IsBanned flags  
        if (user.IsDeleted)
            throw new UnauthorizedException(
                "This account has been deactivated.");

        if (user.IsBanned)
            throw new UnauthorizedException(
                $"This account has been suspended. Reason: {user.BanReason}");

        // 5. Generate Tokens
        var accessToken = await _tokenService.CreateAccessTokenAsync(user);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return MapToDto(user, accessToken, refreshToken.Token, roles);
    }


    public async Task RevokeTokenAsync(
        Guid userId, string refreshToken,
        CancellationToken ct = default)
    {
        var token = await _tokenService
            .GetActiveRefreshTokenAsync(userId, refreshToken, ct)
            ?? throw new BadRequestException(
                "Token not found or already revoked.");

        await _tokenService.RevokeRefreshTokenAsync(token, ct);
    }

    public async Task ResendOtpAsync(
        string email, OtpPurpose purpose,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new NotFoundException("User", email);

        switch (purpose)
        {
            case OtpPurpose.EmailVerification:
                if (user.EmailConfirmed)
                    throw new BadRequestException("Email is already verified.");

                var verifyOtp = await _otpService
                    .GenerateAndSaveOtpAsync(user.Id, ct);

                await _publisher.PublishAsync(new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    OtpCode = verifyOtp
                }, "user.registered", ct);
                break;

            case OtpPurpose.PasswordReset:
                if (!user.EmailConfirmed || user.IsDeleted)
                    return;  

                var resetOtp = await _otpService
                    .GenerateAndSaveOtpAsync(user.Id, ct);

                await _publisher.PublishAsync(new PasswordResetRequestedEvent
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    OtpCode = resetOtp
                }, "password.reset.requested", ct);
                break;
        }
    }

    // ─── Private Helper ───────────────────────────────────────────────────
    private static AuthResponseDto MapToDto(
        AppUser user, string accessToken,
        string refreshToken, IList<string> roles)
        => new()
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Roles = roles
        };


    private async Task<string> GenerateUniqueUsernameAsync(
        string firstName, string lastName)
    {
        // sara.ahmed
        var baseUsername = $"{firstName.ToLower()}.{lastName.ToLower()}"
            .Replace(" ", "")
            .Normalize();

        if (await _userManager.FindByNameAsync(baseUsername) is null)
            return baseUsername;

        // if "sara.ahmed" already exists, try "sara.ahmed.1234" with random 4-digit suffix until we find a unique one
        // sara.ahmed.4821
        string candidate;
        do
        {
            var suffix = Random.Shared.Next(1000, 9999);
            candidate = $"{baseUsername}.{suffix}";
        }
        while (await _userManager.FindByNameAsync(candidate) is not null);

        return candidate;
    }
}