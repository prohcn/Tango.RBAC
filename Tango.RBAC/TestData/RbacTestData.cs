using Microsoft.EntityFrameworkCore;
using Tango.RBAC.RbacServicePackage.Data;
using Tango.RBAC.RbacServicePackage.Models;

namespace Tango.RBAC.TestData
{
    public static class RbacTestData
    {
        public static async Task SeedTestDataAsync(RbacDbContext db)
        {
            // Skip seeding if data already exists
            if (await db.Users.AnyAsync()) return;

            // Clear existing data
            db.Users.RemoveRange(db.Users);
            db.Roles.RemoveRange(db.Roles);
            db.Permissions.RemoveRange(db.Permissions);
            db.AreaTypes.RemoveRange(db.AreaTypes);
            db.PermissionTypes.RemoveRange(db.PermissionTypes);
            await db.SaveChangesAsync();

            // Seed AreaTypes
            var area1 = new AreaType { AreaTypeName = "ODAG", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };
            var area2 = new AreaType { AreaTypeName = "Documents", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };

            db.AreaTypes.AddRange(area1, area2);

            // Seed PermissionTypes
            var perm1 = new PermissionType { PermissionTypeName = "Read", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };
            var perm2 = new PermissionType {    PermissionTypeName = "Write", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };

            db.PermissionTypes.AddRange(perm1, perm2);

            await db.SaveChangesAsync();

            // Seed Permissions
            var permission1 = new Permission { AreaTypeId = area1.AreaTypeId, PermissionTypeId = perm1.PermissionTypeId, Instance = "ODAG_read", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };
            var permission2 = new Permission { AreaTypeId = area1.AreaTypeId, PermissionTypeId = perm2.PermissionTypeId, Instance = "ODAG_write", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };
            var permission3 = new Permission { AreaTypeId = area2.AreaTypeId, PermissionTypeId = perm1.PermissionTypeId, Instance = "Documents_read", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };

            db.Permissions.AddRange(permission1, permission2, permission3);

            // Seed Roles
            var role1 = new Role { RoleName = "ODAG_Admin" };
            var role2 = new Role { RoleName = "Document_Read" };

            db.Roles.AddRange(role1, role2);

            // Seed Users
            var user1 = new User { FirstName = "Alice", LastName = "Smith", Email = "Alice.Smith@test.com", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com"};
            var user2 = new User { FirstName = "Bob", LastName = "Jones", Email = "Bob.Jones@test.com", DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" };

            db.Users.AddRange(user1, user2);

            await db.SaveChangesAsync();

            // Assign Roles to Users
            db.UserRoles.AddRange(
                new UserRole { UserId = user1.UserId, RoleId = role1.RoleId, DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" },
                new UserRole { UserId = user2.UserId, RoleId = role2.RoleId, DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" }
            );

            // Assign Permissions to Roles
            db.RolePermissions.AddRange(
                new RolePermission { RoleId = role1.RoleId, PermissionId = permission1.PermissionId, DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" },
                new RolePermission { RoleId = role1.RoleId, PermissionId = permission2.PermissionId, DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" },
                new RolePermission { RoleId = role2.RoleId, PermissionId = permission3.PermissionId, DateCreated = DateTime.UtcNow, UserCreated = "bmorandi@tangocare.com" }
            );

            await db.SaveChangesAsync();
        }
    }
}