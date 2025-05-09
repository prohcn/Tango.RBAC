
# Tango.RBAC

Tango.RBAC is a reusable, generic Role-Based Access Control (RBAC) system for .NET applications. It supports assigning roles to users, associating permissions to roles, and checking access control using a clean, extensible structure.

---

## 📁 Project Structure

```
Tango.RBAC/
├── Tango.RBAC/              # Core RBAC library (NuGet package target)
│   ├── Models/              # EF Core models like User, Role, Permission, etc.
│   ├── Data/                # RbacDbContext and migrations
│   ├── Services/            # AuthorizationService implementation
│   └── Tango.RBAC.csproj    # Project file for packaging
├── RbacDemoApp/             # ASP.NET Core demo app
│   └── Program.cs           # Minimal API for testing
├── Tango.RBAC.sln           # Solution file
```

## 📦 Projects

- `RbacServicePackage`: The core RBAC library (class library, NuGet-ready)
- `RbacDemoApp`: A minimal ASP.NET Core Web API showcasing usage of the RBAC package

---

## 🛠️ Setup Instructions

### 1. Clone the Repo

```bash
git clone https://github.com/your-org/Tango.RBAC.git
cd Tango.RBAC
```

### 2. Set Up the Database

Update the connection string in `appsettings.Development.json` of `RbacDemoApp`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database={EnterDBHere};Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## ⚙️ EF Core Commands

To run Entity Framework Core commands from the root directory:

If you are in top level Tango.RBAC directory, then run the command
```bash
dotnet ef database update --project Tango.RBAC/Tango.RBAC.csproj --startup-project RbacDemoApp/RbacDemoApp.csproj
```

###(Optionally) Add a migration:

```bash
dotnet ef migrations add MigrationName \
  --project Tango.RBAC \
  --startup-project RbacDemoApp
```

### Update the database:

```bash
dotnet ef database update \
  --project Tango.RBAC \
  --startup-project RbacDemoApp
```

> Note: `Tango.RBAC` must be an SDK-style project and reference Microsoft.EntityFrameworkCore.Design.

---

## 📦 Create NuGet Package

### 1. Pack the library:

```bash
dotnet pack Tango.RBAC/Tango.RBAC.csproj -c Release
```

### 2. Push to NuGet:

```bash
dotnet nuget push bin/Release/Tango.RBAC.*.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

---

## 🚀 Usage

1. Reference the `Tango.RBAC` package in your project.
2. Add the `RbacDbContext` to your DI container.
3. Use the `IAuthorizationService` to check permissions:

```csharp
var hasAccess = await authorizationService.HasPermissionAsync(userId, areaTypeId, permissionTypeId);
```

---

## 🧪 Test Data

A static seeder `RbacTestData.SeedTestDataAsync()` is available to populate your database with initial test data for demo/testing purposes.

---
