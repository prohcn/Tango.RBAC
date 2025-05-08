using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Tango.RBAC.RbacServicePackage.Data;
using Tango.RBAC.RbacServicePackage.Models;
using Tango.RBAC.Services;

namespace Tango.RBAC.RbacServicePackage.Tests.Services
{
    public class AuthorizationServiceTests
    {
        private RbacDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RbacDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RbacDbContext(options);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenUserHasPermissionViaRole()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { Email = "test@example.com", IsActive = true };
            var role = new Role { RoleName = "Admin", IsActive = true };
            var areaType = new AreaType { AreaTypeName = "TestArea", DateCreated = DateTime.UtcNow, AreaTypeId = 1 };
            var permissionType = new PermissionType { PermissionTypeName = "Read", DateCreated = DateTime.UtcNow, PermissionTypeId = 1 };
            db.Users.Add(user);
            db.Roles.Add(role);
            db.AreaTypes.Add(areaType);
            db.PermissionTypes.Add(permissionType);
            await db.SaveChangesAsync();
            var permission = new Permission { AreaTypeId = areaType.AreaTypeId, PermissionTypeId = permissionType.PermissionTypeId, DateCreated = DateTime.UtcNow };
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId, DateCreated = DateTime.UtcNow });
            db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionId = permission.PermissionId, DateCreated = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            var hasPermission = await service.HasPermissionAsync(user.UserId, areaType.AreaTypeId, permissionType.PermissionTypeId);

            // Assert
            Assert.True(hasPermission);
        }

        [Fact]
        public async Task AssignPermissionToRoleAsync_AddsPermissionToRole()
        {
            var db = GetInMemoryDbContext();
            var role = new Role { RoleName = "Editor", IsActive = true };
            var areaType = new AreaType { AreaTypeName = "Articles", DateCreated = DateTime.UtcNow };
            var permissionType = new PermissionType { PermissionTypeName = "Edit", DateCreated = DateTime.UtcNow };
            var user = "testUser@test.com";
            db.Roles.Add(role);
            db.AreaTypes.Add(areaType);
            db.PermissionTypes.Add(permissionType);
            await db.SaveChangesAsync();
            var permission = new Permission { AreaTypeId = areaType.AreaTypeId, PermissionTypeId = permissionType.PermissionTypeId, DateCreated = DateTime.UtcNow };
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.AssignPermissionToRoleAsync(role.RoleId, permission.PermissionId, user);

            var rolePermission = await db.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == permission.PermissionId);
            Assert.NotNull(rolePermission);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_AddsRoleToUser()
        {
            var db = GetInMemoryDbContext();
            var user = new User { Email = "assignrole@example.com", IsActive = true };
            var role = new Role { RoleName = "Contributor", IsActive = true };
            var userCreated = "testUser@test.com";
            db.Users.Add(user);
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.AssignRoleToUserAsync(user.UserId, role.RoleId, userCreated);

            var userRole = await db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.UserId && ur.RoleId == role.RoleId);
            Assert.NotNull(userRole);
        }

        [Fact]
        public async Task AssignPermissionToRoleAsync_DoesNotDuplicate()
        {
            var db = GetInMemoryDbContext();
            var role = new Role { RoleName = "Publisher", IsActive = true };
            var areaType = new AreaType { AreaTypeName = "Articles", DateCreated = DateTime.UtcNow };
            var permissionType = new PermissionType { PermissionTypeName = "Publish", DateCreated = DateTime.UtcNow };
            var userCreated = "testUser@test.com";
            db.Roles.Add(role);
            db.AreaTypes.Add(areaType);
            db.PermissionTypes.Add(permissionType);
            await db.SaveChangesAsync();

            var permission = new Permission { AreaTypeId = areaType.AreaTypeId, PermissionTypeId = permissionType.PermissionTypeId, DateCreated = DateTime.UtcNow };
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.AssignPermissionToRoleAsync(role.RoleId, permission.PermissionId, userCreated);
            await service.AssignPermissionToRoleAsync(role.RoleId, permission.PermissionId, userCreated);

            var count = await db.RolePermissions.CountAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == permission.PermissionId);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_DoesNotDuplicate()
        {
            var db = GetInMemoryDbContext();
            var user = new User { Email = "duplicateuserrole@example.com", IsActive = true };
            var role = new Role { RoleName = "Auditor", IsActive = true };
            var userCreated = "testUser@test.com";
            db.Users.Add(user);
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.AssignRoleToUserAsync(user.UserId, role.RoleId, userCreated);
            await service.AssignRoleToUserAsync(user.UserId, role.RoleId, userCreated);

            var count = await db.UserRoles.CountAsync(ur => ur.UserId == user.UserId && ur.RoleId == role.RoleId);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task ChangeUserRole_RemovesOldRoleAndAddsNewRole()
        {
            var db = GetInMemoryDbContext();
            var user = new User { Email = "changeuserrole@example.com", IsActive = true };
            var oldRole = new Role { RoleName = "OldRole", IsActive = true };
            var newRole = new Role { RoleName = "NewRole", IsActive = true };
            var userCreated = "testUser@test.com";
            db.Users.Add(user);
            db.Roles.AddRange(oldRole, newRole);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = oldRole.RoleId });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            var existingUserRole = await db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.UserId && ur.RoleId == oldRole.RoleId);
            if (existingUserRole != null)
            {
                db.UserRoles.Remove(existingUserRole);
                await db.SaveChangesAsync();
            }

            await service.AssignRoleToUserAsync(user.UserId, newRole.RoleId, userCreated);

            var oldRoleExists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == oldRole.RoleId);
            var newRoleExists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == newRole.RoleId);

            Assert.False(oldRoleExists);
            Assert.True(newRoleExists);
        }
    }
}