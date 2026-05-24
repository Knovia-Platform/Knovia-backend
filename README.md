# Knovia Platform — Backend API

<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=for-the-badge&logo=postgresql)
![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?style=for-the-badge&logo=redis)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-FF6600?style=for-the-badge&logo=rabbitmq)
![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?style=for-the-badge&logo=stripe)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)

**A production-grade e-learning platform backend built with Clean Architecture, CQRS, and modern .NET practices.**

[Features](#-features) • [Architecture](#-architecture) • [Tech Stack](#-tech-stack) • [Getting Started](#-getting-started) • [API Docs](#-api-documentation) • [Modules](#-modules)

</div>

---

## 📌 Overview

Knovia Platform is a full-featured online course platform (Udemy-like) built with **ASP.NET Core 8** following **Clean Architecture** principles. It supports course creation, student enrollment, payments,  certificates, and much more — all designed to be scalable, maintainable, and production-ready.

---

## ✨ Features

| Feature                  | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| 🔐 **Authentication**    | JWT + Refresh Tokens, Google OAuth, OTP Email Verification   |
| 📚 **Course Management** | Full CRUD, Status Workflow (Draft → Review → Published)      |
| 🎓 **Enrollment**        | Instant enrollment after payment, free course support        |
| 📊 **Progress Tracking** | Lesson completion, watch-time tracking, auto-complete at 80% |
| 💳 **Payments**          | Stripe integration, coupon system, order management          |
| 📜 **Certificates**      | PDF generation (QuestPDF), unique verification codes         |
| ⭐ **Reviews & Ratings** | Rating system with average calculation                       |
| 🔔 **Notifications**     | In-app + Email via RabbitMQ event-driven architecture        |
| 💰 **Payouts**           | Stripe Connect for instructor payouts with 70/30 split       |
| 📈 **Dashboards**        | Instructor analytics + Admin platform statistics             |
| 👤 **Admin Panel**       | User management, content moderation, platform stats          |
| 🏷️ **Coupons**           | Percentage & fixed-amount discounts with usage limits        |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│                    API Layer                        │
│         Controllers • Middleware • Extensions       │
└─────────────────────┬───────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────┐
│                Application Layer                    │
│   CQRS (Commands/Queries) • MediatR • FluentValidation │
│   Specifications • DTOs • AutoMapper • Behaviors    │
└─────────────────────┬───────────────────────────────┘
                      │
┌──────────┬──────────▼──────────┬────────────────────┐
│  Domain  │                     │   Infrastructure   │
│  Layer   │                     │       Layer        │
│          │                     │                    │
│ Entities │                     │  EF Core • Redis   │
│  Enums   │                     │  RabbitMQ • Stripe │
│ Interfaces│                    │  Cloudinary• Email │
└──────────┴─────────────────────┴────────────────────┘
```

### Design Patterns Used

- **Clean Architecture** — strict layer separation with dependency inversion
- **CQRS** — Commands and Queries separated via MediatR
- **Repository Pattern** — Generic repository with specification support
- **Unit of Work** — Atomic operations across multiple repositories
- **Specification Pattern** — Reusable, composable query logic
- **Observer Pattern** — Event-driven notifications via RabbitMQ

---

## 🛠️ Tech Stack

### Core

| Technology              | Purpose                     |
| ----------------------- | --------------------------- |
| ASP.NET Core 8          | Web API framework           |
| Entity Framework Core 8 | ORM & database access       |
| PostgreSQL              | Primary database            |
| MediatR                 | CQRS & in-process messaging |
| FluentValidation        | Request validation          |
| AutoMapper              | Object mapping              |

### Infrastructure

| Technology       | Purpose                                |
| ---------------- | -------------------------------------- |
| Redis            | Caching (courses, categories)          |
| RabbitMQ         | Async messaging (email, notifications) |
| Stripe           | Payment processing & subscriptions     |
| Stripe Connect   | Instructor payout system               |
| QuestPDF         | PDF certificate generation             |
| ASP.NET Identity | Authentication & authorization         |
| JWT              | Token-based authentication             |
| Cloudinary       | Media storage & image/file management  |

---

## 📁 Project Structure

```
CoursePlatform/
├── CoursePlatform.Domain/
│   ├── Entities/          # Core business entities
│   ├── Enums/             # Domain enumerations
│   └── Common/            # Base classes (BaseEntity, AuditableEntity)
│
├── CoursePlatform.Application/
│   ├── Features/          # CQRS handlers per feature
│   │   ├── Auth/
│   │   ├── Courses/
│   │   ├── Curriculum/
│   │   ├── Orders/
│   │   ├── Enrollments/
│   │   ├── Progress/
│   │   ├── Reviews/
│   │   ├── Certificates/
│   │   ├── Coupons/
│   │   ├── Notifications/
│   │   ├── Payouts/
│   │   ├── Subscriptions/
│   │   ├── InstructorDashboard/
│   │   └── Admin/
│   ├── Contracts/         # Interfaces (repositories, services)
│   ├── Specifications/    # Reusable query specifications
│   └── Common/            # Behaviors, exceptions, models
│
├── CoursePlatform.Infrastructure/
│   ├── Persistence/       # DbContext, configurations, migrations
│   ├── Services/          # External service implementations
│   └── Consumers/         # RabbitMQ background consumers
│
└── CoursePlatform.API/
    ├── Controllers/        # API endpoints
    ├── Middleware/         # Exception handling
    └── Extensions/        # Service registration
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL](https://www.postgresql.org/) (or Docker)
- [Redis](https://redis.io/) or Docker
- [RabbitMQ](https://www.rabbitmq.com/) or Docker

### 1. Clone the Repository

```bash
git clone https://github.com/Knovia-Platform/Knovia-backend.git
cd Knovia-backend
```

### 2. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GuidyPlatform;Trusted_Connection=True;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters",
    "Issuer": "GuidyPlatform",
    "Audience": "GuidyPlatformUsers",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "DisplayName": "Guidy Platform"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Cloudinary": {
    "CloudName": "",
    "ApiKey": "",
    "ApiSecret": ""
  }
}
```

### 3. Run with Docker (Recommended)

```bash
# Start infrastructure services
docker-compose up -d

# Apply migrations
dotnet ef database update --project CoursePlatform.Infrastructure --startup-project CoursePlatform.API

# Run the API
dotnet run --project CoursePlatform.API
```

### 4. Run Locally

```bash
# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update \
  --project CoursePlatform.Infrastructure \
  --startup-project CoursePlatform.API

# Run the API
dotnet run --project CoursePlatform.API
```

The API will be available at `https://localhost:7022`

Swagger UI: `https://localhost:7022/swagger`

---

## 📖 API Documentation

### Authentication

| Method | Endpoint                    | Description               | Auth |
| ------ | --------------------------- | ------------------------- | ---- |
| POST   | `/api/auth/register`        | Register new account      | ❌   |
| POST   | `/api/auth/login`           | Login with email/password | ❌   |
| POST   | `/api/auth/refresh-token`   | Refresh access token      | ❌   |
| POST   | `/api/auth/verify-email`    | Verify email with OTP     | ❌   |
| POST   | `/api/auth/forgot-password` | Request password reset    | ❌   |
| POST   | `/api/auth/reset-password`  | Reset password with OTP   | ❌   |
| POST   | `/api/auth/google-login`    | Google OAuth login        | ❌   |
| GET    | `/api/auth/me`              | Get current user info     | ✅   |

### Courses

| Method | Endpoint                      | Description                       | Role             |
| ------ | ----------------------------- | --------------------------------- | ---------------- |
| GET    | `/api/courses`                | Get published courses (paginated) | Public           |
| GET    | `/api/courses/{id}`           | Get course details                | Public           |
| GET    | `/api/courses/my`             | Get instructor's courses          | Instructor       |
| POST   | `/api/courses`                | Create new course                 | Instructor       |
| PUT    | `/api/courses/{id}`           | Update course                     | Instructor       |
| DELETE | `/api/courses/{id}`           | Soft delete course                | Instructor       |
| POST   | `/api/courses/{id}/submit`    | Submit for review                 | Instructor       |
| PUT    | `/api/courses/{id}/approve`   | Approve course                    | Admin            |
| PUT    | `/api/courses/{id}/reject`    | Reject course                     | Admin            |
| PUT    | `/api/courses/{id}/archive`   | Archive course                    | Instructor/Admin |
| PUT    | `/api/courses/{id}/unarchive` | Unarchive course                  | Instructor/Admin |

### Curriculum

| Method | Endpoint                                                                   | Description           | Role            |
| ------ | -------------------------------------------------------------------------- | --------------------- | --------------- |
| GET    | `/api/courses/{id}/curriculum`                                             | Get course curriculum | Public/Enrolled |
| POST   | `/api/courses/{id}/curriculum/sections`                                    | Add section           | Instructor      |
| PUT    | `/api/courses/{id}/curriculum/sections/{sId}`                              | Update section        | Instructor      |
| DELETE | `/api/courses/{id}/curriculum/sections/{sId}`                              | Delete section        | Instructor      |
| PUT    | `/api/courses/{id}/curriculum/sections/reorder`                            | Reorder sections      | Instructor      |
| POST   | `/api/courses/{id}/curriculum/sections/{sId}/lessons`                      | Add lesson            | Instructor      |
| PUT    | `/api/courses/{id}/curriculum/sections/{sId}/lessons/{lId}`                | Update lesson         | Instructor      |
| DELETE | `/api/courses/{id}/curriculum/sections/{sId}/lessons/{lId}`                | Delete lesson         | Instructor      |
| PATCH  | `/api/courses/{id}/curriculum/sections/{sId}/lessons/{lId}/toggle-preview` | Toggle free preview   | Instructor      |

### Orders & Payments

| Method | Endpoint                     | Description        | Role    |
| ------ | ---------------------------- | ------------------ | ------- |
| POST   | `/api/orders`                | Create order       | Student |
| POST   | `/api/orders/{id}/coupon`    | Apply coupon       | Student |
| GET    | `/api/orders/my`             | Get my orders      | Student |
| GET    | `/api/orders/my/enrollments` | Get my enrollments | Student |
| POST   | `/api/payments/webhook`      | Stripe webhook     | Stripe  |

### Progress & Certificates

| Method | Endpoint                                              | Description          | Role    |
| ------ | ----------------------------------------------------- | -------------------- | ------- |
| GET    | `/api/courses/{id}/progress`                          | Get course progress  | Student |
| POST   | `/api/courses/{id}/progress/lessons/{lId}/complete`   | Mark lesson complete | Student |
| POST   | `/api/courses/{id}/progress/lessons/{lId}/watch-time` | Update watch time    | Student |
| POST   | `/api/certificates/courses/{courseId}`                | Issue certificate    | Student |
| GET    | `/api/certificates`                                   | Get my certificates  | Student |
| GET    | `/api/certificates/{id}/download`                     | Download PDF         | Student |
| GET    | `/api/certificates/verify/{code}`                     | Verify certificate   | Public  |

### Subscriptions

| Method | Endpoint                   | Description            | Role    |
| ------ | -------------------------- | ---------------------- | ------- |
| GET    | `/api/subscriptions/plans` | Get subscription plans | Public  |
| GET    | `/api/subscriptions/my`    | Get my subscription    | Student |
| POST   | `/api/subscriptions`       | Subscribe to plan      | Student |
| DELETE | `/api/subscriptions/my`    | Cancel subscription    | Student |

### Instructor Dashboard

| Method | Endpoint                                | Description            | Role       |
| ------ | --------------------------------------- | ---------------------- | ---------- |
| GET    | `/api/instructor/dashboard/summary`     | Dashboard overview     | Instructor |
| GET    | `/api/instructor/dashboard/revenue`     | Revenue analytics      | Instructor |
| GET    | `/api/instructor/dashboard/enrollments` | Enrollment analytics   | Instructor |
| GET    | `/api/instructor/dashboard/top-courses` | Top performing courses | Instructor |

### Admin

| Method | Endpoint                      | Description         | Role  |
| ------ | ----------------------------- | ------------------- | ----- |
| GET    | `/api/admin/stats`            | Platform statistics | Admin |
| GET    | `/api/admin/users`            | All users           | Admin |
| PATCH  | `/api/admin/users/{id}/ban`   | Ban user            | Admin |
| PATCH  | `/api/admin/users/{id}/unban` | Unban user          | Admin |
| PATCH  | `/api/admin/users/{id}/role`  | Change user role    | Admin |
| GET    | `/api/admin/courses`          | All courses         | Admin |

---

## 🔄 Key Workflows

### Course Publication Flow

```
Draft → Submit → UnderReview → Approve → Published
                            → Reject  → Draft (with reason)
Published → Archive → Archived → Unarchive → Draft
```

### Payment Flow

```
Create Order → Apply Coupon (optional) → Stripe Payment Intent
→ Frontend completes payment → Stripe Webhook
→ Order Completed → Instant Enrollment → Notification sent
```

### Certificate Flow

```
Complete all lessons (100%) → Issue Certificate
→ Generate PDF (QuestPDF) → Unique verify code
→ Public verification endpoint
```

---

## 🌱 Seed Data

The application seeds the following test data on first run (Development):

**10 seeded courses** across categories: Web Development, Data Science, Machine Learning, Business.

**4 seeded coupons:**

- `SAVE20` — 20% discount
- `FLAT10` — $10 off
- `FREECOURSE` — 100% off
- `EXPIRED` — expired coupon (for testing)

---

## 🔒 Security Features

- JWT authentication with refresh token rotation
- OTP-based email verification (no magic links)
- Role-based authorization (Student, Instructor, Admin)
- File upload validation (extension + MIME type + magic bytes)
- Path traversal attack prevention
- Stripe webhook signature validation
- Soft delete for sensitive data preservation
- Audit logging on all entities

---

## ⚙️ Environment Variables

| Variable                               | Description                    |
| -------------------------------------- | ------------------------------ |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string   |
| `ConnectionStrings__Redis`             | Redis connection string        |
| `Jwt__Key`                             | JWT signing key (min 32 chars) |
| `Stripe__SecretKey`                    | Stripe secret key              |
| `Stripe__WebhookSecret`                | Stripe webhook signing secret  |
| `RabbitMQ__Host`                       | RabbitMQ host                  |
| `Email__Username`                      | SMTP email address             |
| `Email__Password`                      | SMTP password/app password     |
| `Cloudinary__CloudName`                | Cloudinary account cloud name  |
| `Cloudinary__ApiKey`                   | Cloudinary API key             |
| `Cloudinary__ApiSecret`                | Cloudinary API secret          |

---

## 📄 License

This project is licensed under the MIT License.

---

## 👩‍💻 Author

**Aya Samir Selim**
Backend Developer | Computer Engineering Student

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0A66C2?style=flat&logo=linkedin)](https://www.linkedin.com/in/ayasamirselim/?locale=en_US)
[![GitHub](https://img.shields.io/badge/GitHub-Follow-181717?style=flat&logo=github)](https://github.com/Aya-Selim)

---

<div align="center">
Built using ASP.NET Core & Clean Architecture
</div>
