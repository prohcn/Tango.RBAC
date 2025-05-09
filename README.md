
# Tango.RBAC

**Tango.RBAC** is a modular, extensible Role-Based Access Control (RBAC) system built for .NET 8. It supports user/role/permission relationships with overrides, effective dates, and auditing.

---

## 🚀 Features

- User/Role/Permission data model
- Supports:
  - Role-to-user assignments
  - Permission-to-role assignments
  - Direct user permission overrides (GRANT/DENY)
- EffectiveFrom / EffectiveThrough time spans
- Soft-deletes (`IsActive`, `IsRetired`)
- Audit support
- Includes seeding and testing endpoints
- Swagger UI enabled for API interaction

---

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

Run EF Core migrations (or ensure your database matches the schema in the source).

If you are in top level Tango.RBAC directory, then run the command
```bash
dotnet ef database update --project Tango.RBAC/Tango.RBAC.csproj --startup-project RbacDemoApp/RbacDemoApp.csproj
```

---

### 3. Run the Demo App

```bash
dotnet run --project RbacDemoApp
```

Then visit:

```
https://localhost:{port}/swagger
```

---

> 📝 Full Swagger documentation is available at `/swagger`.

---

## 🧪 Seeding Test Data

Modify or call the `RbacTestData.SeedTestDataAsync(RbacDbContext)` method in `Program.cs` of `RbacDemoApp` to add test users, roles, and permissions.
