// Infrastructure/Services/CloudinaryStorageService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CoursePlatform.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Infrastructure.Services;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(
        IConfiguration config,
        ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;

        var account = new Account(
            config["Cloudinary:CloudName"]!,
            config["Cloudinary:ApiKey"]!,
            config["Cloudinary:ApiSecret"]!);

        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string folder,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLower();

        // صور → ImageUploadParams
        // ملفات تانية (PDF, etc) → RawUploadParams
        if (IsImage(extension))
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"guidy/{folder}",
                UniqueFilename = true,
                Overwrite = false,
                // Transformation للـ thumbnails
                Transformation = folder == "thumbnails"
                    ? new Transformation()
                        .Width(640).Height(360)
                        .Crop("fill").Quality("auto")
                    : null
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            _logger.LogInformation(
                "Image uploaded to Cloudinary: {Url}", result.SecureUrl);

            return result.SecureUrl.ToString();
        }
        else
        {
            // PDF و Resources
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"guidy/{folder}",
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            _logger.LogInformation(
                "File uploaded to Cloudinary: {Url}", result.SecureUrl);

            return result.SecureUrl.ToString();
        }
    }

    public async Task DeleteAsync(
        string fileUrl,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            // استخرجي الـ PublicId من الـ URL
            // مثال URL: https://res.cloudinary.com/cloud/image/upload/v123/guidy/thumbnails/abc.jpg
            // PublicId: guidy/thumbnails/abc
            var uri = new Uri(fileUrl);
            var segments = uri.AbsolutePath.Split('/');

            // ابحثي عن "guidy" في الـ segments
            var guidyIndex = Array.IndexOf(segments, "guidy");
            if (guidyIndex < 0) return;

            var publicIdWithExt = string.Join("/",
                segments[guidyIndex..]);
            var publicId = Path.ChangeExtension(
                publicIdWithExt, null);

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            _logger.LogInformation(
                "File deleted from Cloudinary: {PublicId}, Result: {Result}",
                publicId, result.Result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to delete from Cloudinary: {Url}", fileUrl);
        }
    }

    public string GetFullUrl(string relativeUrl, string baseUrl)
        => relativeUrl; // Cloudinary بيرجع Full URL مباشرة

    private static bool IsImage(string extension)
        => extension is ".jpg" or ".jpeg" or ".png"
                     or ".gif" or ".webp" or ".svg";
}