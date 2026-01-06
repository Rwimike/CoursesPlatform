# Course Platform

A full-stack course management system built with .NET 8, PostgreSQL, and Vue.js. This platform allows users to create, manage, and organize courses with lessons, featuring authentication, CRUD operations, and soft-delete functionality.

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [Backend Setup](#backend-setup)
- [Frontend Setup](#frontend-setup)
- [Running the Application](#running-the-application)
- [Test Credentials](#test-credentials)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Testing](#testing)

## ✨ Features

- **User Authentication**: JWT-based authentication with ASP.NET Identity
- **Course Management**: Create, read, update, and delete courses
- **Lesson Management**: Organize lessons within courses with custom ordering
- **Soft Delete**: Courses and lessons are soft-deleted, preserving data integrity
- **Publishing System**: Draft and publish courses (requires at least one lesson)
- **Search & Filtering**: Search courses by title and filter by status
- **Responsive Dashboard**: View metrics and recent activity
- **Clean Architecture**: Separation of concerns with Core, Infrastructure, and API layers

## 🛠 Tech Stack

### Backend
- **.NET 8.0** - Web API
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL 15** - Database
- **ASP.NET Identity** - Authentication
- **JWT Bearer** - Token-based authentication
- **Swagger/OpenAPI** - API documentation

### Frontend
- **Vue.js 3** - Progressive JavaScript framework
- **Axios** - HTTP client
- **Tailwind CSS** - Utility-first CSS framework

### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/get-started) (optional, for containerized database)
- [Node.js](https://nodejs.org/) (optional, for serving frontend with a local server)

## 🗄️ Database Setup

### Option 1: Using Docker (Recommended)

1. **Start PostgreSQL with Docker Compose:**
   ```bash
   docker-compose up -d
   ```

   This will start a PostgreSQL container with:
   - **Database**: `courseplatform`
   - **User**: `postgres`
   - **Password**: `postgres`
   - **Port**: `5432`

2. **Verify the container is running:**
   ```bash
   docker ps
   ```

### Option 2: Manual PostgreSQL Installation

1. **Install PostgreSQL 15+** on your system

2. **Create the database:**
   ```sql
   CREATE DATABASE courseplatform;
   ```

3. **Update connection string** (if needed) in `CoursePlatform.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=courseplatform;Username=postgres;Password=YOUR_PASSWORD"
     }
   }
   ```

## 🚀 Backend Setup

### 1. Navigate to the API project:
```bash
cd CoursePlatform.API
```

### 2. Restore dependencies:
```bash
dotnet restore
```

### 3. Run database migrations:
```bash
dotnet ef database update
```

This will:
- Create all necessary tables (Courses, Lessons, AspNetUsers, etc.)
- Apply database constraints and indexes
- Seed a test user account

### 4. Build the project:
```bash
dotnet build
```

### 5. Run the API:
```bash
dotnet run
```

The API will start at:
- **HTTP**: `http://localhost:5054`
- **HTTPS**: `https://localhost:7277` (if configured)
- **Swagger UI**: `http://localhost:5054/swagger`

## 🎨 Frontend Setup

### 1. Navigate to the frontend directory:
```bash
cd FrontEnd
```

### 2. Verify the API URL

Open `FrontEnd/App.js` and confirm the API URL matches your backend:
```javascript
const API_URL = 'http://localhost:5054/api';
```

### 3. Serve the frontend

**Option A: Using a simple HTTP server (recommended)**
```bash
# Install http-server globally (once)
npm install -g http-server

# Serve the frontend
http-server -p 8080
```

Then open: `http://localhost:8080`

**Option B: Using Python**
```bash
# Python 3
python -m http.server 8080
```

**Option C: Using VS Code Live Server**
- Install the "Live Server" extension
- Right-click on `Index.html`
- Select "Open with Live Server"

**Option D: Open directly in browser**
- Simply open `FrontEnd/Index.html` in your web browser
- Note: Some features may require a proper HTTP server

## 🎮 Running the Application

### Complete Startup Sequence

1. **Start the database** (if using Docker):
   ```bash
   docker-compose up -d
   ```

2. **Start the backend API**:
   ```bash
   cd CoursePlatform.API
   dotnet run
   ```
   
   Wait for: `Now listening on: http://localhost:5054`

3. **Start the frontend**:
   ```bash
   cd FrontEnd
   http-server -p 8080
   ```

4. **Access the application**:
   - Frontend: `http://localhost:8080`
   - API Swagger: `http://localhost:5054/swagger`

## 🔑 Test Credentials

The application comes with a pre-seeded test user:

```
Email: test@courseplatform.com
Password: Test123!
```

These credentials are automatically created during database migration.

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/register` | Register a new user |
| POST | `/api/Auth/login` | Login and receive JWT token |

### Course Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Courses/search` | Search and filter courses | ✅ |
| GET | `/api/Courses/{id}` | Get course by ID | ✅ |
| GET | `/api/Courses/{id}/summary` | Get course summary with lesson count | ✅ |
| POST | `/api/Courses` | Create a new course | ✅ |
| PUT | `/api/Courses/{id}` | Update course | ✅ |
| DELETE | `/api/Courses/{id}` | Soft delete course | ✅ |
| PATCH | `/api/Courses/{id}/publish` | Publish course | ✅ |
| PATCH | `/api/Courses/{id}/unpublish` | Unpublish course | ✅ |

### Lesson Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/courses/{courseId}/Lessons` | Get all lessons for a course | ✅ |
| GET | `/api/courses/{courseId}/Lessons/{id}` | Get lesson by ID | ✅ |
| POST | `/api/courses/{courseId}/Lessons` | Create a new lesson | ✅ |
| PUT | `/api/courses/{courseId}/Lessons/{id}` | Update lesson | ✅ |
| DELETE | `/api/courses/{courseId}/Lessons/{id}` | Soft delete lesson | ✅ |
| PATCH | `/api/courses/{courseId}/Lessons/{id}/reorder` | Reorder lesson | ✅ |

### Request Examples

**Login:**
```bash
curl -X POST http://localhost:5054/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@courseplatform.com",
    "password": "Test123!"
  }'
```

**Create Course:**
```bash
curl -X POST http://localhost:5054/api/Courses \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "title": "Introduction to Vue.js"
  }'
```

**Search Courses:**
```bash
curl -X GET "http://localhost:5054/api/Courses/search?page=1&pageSize=10&status=1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📁 Project Structure

```
CoursePlatform/
├── CoursePlatform.API/              # Web API Layer
│   ├── Controllers/                 # API Controllers
│   ├── Properties/
│   │   └── launchSettings.json     # Launch configuration
│   ├── Program.cs                  # Application entry point
│   └── appsettings.json            # Configuration
│
├── CoursePlatform.Core/             # Domain Layer
│   ├── Entities/                   # Domain entities
│   │   ├── Course.cs
│   │   └── Lesson.cs
│   ├── Interfaces/                 # Repository interfaces
│   │   └── IRepository.cs
│   └── Services/                   # Business logic
│       ├── CourseService.cs
│       └── LessonService.cs
│
├── CoursePlatform.Infrastructure/   # Data Access Layer
│   ├── Data/
│   │   └── ApplicationDbContext.cs # EF Core DbContext
│   ├── Migrations/                 # EF Core migrations
│   └── Repositories/               # Repository implementations
│       ├── CourseRepository.cs
│       ├── LessonRepository.cs
│       └── UnitOfWork.cs
│
├── CoursePlatform.Tests/            # Unit Tests
│   └── UnitTest1.cs
│
├── FrontEnd/                        # Vue.js Frontend
│   ├── App.js                      # Vue application logic
│   └── Index.html                  # Main HTML file
│
└── docker-compose.yml               # Docker configuration
```

## 🧪 Testing

### Run Unit Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Test Coverage

The test suite includes:
- ✅ Course creation and validation
- ✅ Course publishing rules (requires lessons)
- ✅ Lesson ordering and uniqueness
- ✅ Soft delete functionality
- ✅ Service layer business logic

## 🔧 Configuration

### JWT Settings

Configure JWT in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "CoursePlatform",
    "Audience": "CoursePlatformUsers"
  }
}
```

### CORS Policy

The API is configured to allow all origins for development. For production, update the CORS policy in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://your-frontend-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## 🐛 Troubleshooting

### Common Issues

**1. Database connection failed**
- Verify PostgreSQL is running: `docker ps` or check your local PostgreSQL service
- Check connection string in `appsettings.json`
- Ensure database exists: `CREATE DATABASE courseplatform;`

**2. Migration errors**
```bash
# Remove all migrations and start fresh
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**3. CORS errors in frontend**
- Verify the API is running on the expected port
- Check that CORS is properly configured in `Program.cs`
- Ensure `API_URL` in `App.js` matches your backend URL

**4. JWT authentication errors**
- Verify the JWT secret key is at least 32 characters
- Check that the token hasn't expired (24-hour lifetime by default)
- Ensure the Authorization header format is: `Bearer YOUR_TOKEN`

**5. Port already in use**
```bash
# Find and kill process using port 5054
# Windows
netstat -ano | findstr :5054
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5054 | xargs kill -9
```

## 📝 Additional Commands

### Entity Framework Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove

# Update database to latest migration
dotnet ef database update

# Update to specific migration
dotnet ef database update MigrationName

# Generate SQL script
dotnet ef migrations script

# Drop database
dotnet ef database drop
```

### Docker Commands

```bash
# Start containers
docker-compose up -d

# Stop containers
docker-compose down

# View logs
docker-compose logs -f

# Restart containers
docker-compose restart

# Remove containers and volumes
docker-compose down -v
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/AmazingFeature`
3. Commit your changes: `git commit -m 'Add some AmazingFeature'`
4. Push to the branch: `git push origin feature/AmazingFeature`
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👥 Author

- Miguel Angel Lopera - Mike

## 🙏 Acknowledgments

- ASP.NET Core documentation
- Entity Framework Core documentation
- Vue.js documentation
- Tailwind CSS documentation

---

**Note**: This is a development setup. For production deployment, ensure you:
- Use environment variables for sensitive configuration
- Enable HTTPS
- Implement proper error handling and logging
- Configure production-ready CORS policies
- Use a production-grade database setup
- Implement rate limiting and security headers

## NICE!!!! 🦖
