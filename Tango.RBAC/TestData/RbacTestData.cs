using Tango.RBAC.RbacServicePackage.Models;
using Tango.RBAC.RbacServicePackage.Data;
using Microsoft.EntityFrameworkCore;

namespace Tango.RBAC.TestData
{
    public static class RbacTestData
    {
        public static async Task SeedTestDataAsync(RbacDbContext db)
        {
            if (await db.Users.AnyAsync() || await db.Roles.AnyAsync() || await db.Permissions.AnyAsync())
                return; // Seed only if DB is empty

            // Users
            var user1 = new User
            {
                OID = "OID123",
                Email = "alice@example.com",
                FirstName = "Alice",
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var user2 = new User
            {
                OID = "OID456",
                Email = "bob@example.com",
                FirstName = "Bob",
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var user3 = new User
            {
                OID = "OID789",
                Email = "chris@example.com",
                FirstName = "Chris",
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            db.Users.AddRange(user1, user2, user3);
            await db.SaveChangesAsync(); // Save users first to get their IDs

            // Roles
            var adminRole = new Role
            {
                RoleName = "Admin",
                RoleDescription = "Administrator",
                DateCreated = DateTime.UtcNow
            };

            var viewerRole = new Role
            {
                RoleName = "Viewer",
                RoleDescription = "Can view data",
                DateCreated = DateTime.UtcNow
            };

            db.Roles.AddRange(adminRole, viewerRole);
            await db.SaveChangesAsync(); // Save roles

            // Permissions
            var permRead = new Permission
            {
                Area = "Referrals",
                Name = "Read",
                Description = "Read access to referrals",
                DateCreated = DateTime.UtcNow
            };

            var permWrite = new Permission
            {
                Area = "Referrals",
                Name = "Write",
                Description = "Write access to referrals",
                DateCreated = DateTime.UtcNow
            };

            db.Permissions.AddRange(permRead, permWrite);
            await db.SaveChangesAsync(); // Save permissions

            // RoleAssignments
            db.UserRoles.AddRange(
                new UserRole
                {
                    UserId = user1.UserId,
                    RoleId = adminRole.RoleId,
                    DateCreated = DateTime.UtcNow
                },
                new UserRole
                {
                    UserId = user2.UserId,
                    RoleId = viewerRole.RoleId,
                    DateCreated = DateTime.UtcNow
                }
            );

            db.RolePermissions.AddRange(
                new RolePermission
                {
                    RoleId = adminRole.RoleId,
                    PermissionId = permRead.PermissionId,
                    DateCreated = DateTime.UtcNow
                },
                new RolePermission
                {
                    RoleId = adminRole.RoleId,
                    PermissionId = permWrite.PermissionId,
                    DateCreated = DateTime.UtcNow
                },
                new RolePermission
                {
                    RoleId = viewerRole.RoleId,
                    PermissionId = permRead.PermissionId,
                    DateCreated = DateTime.UtcNow
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
