
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

            var context = new RbacDbContext(options);
            return context;
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenUserHasPermissionViaRole()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u1", Email = "test@example.com", IsActive = true };
            var role = new Role { RoleName = "Admin", IsActive = true };
            var permission = new Permission { Area = "TestArea", Name = "Read" };

            db.Users.Add(user);
            db.Roles.Add(role);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionId = permission.PermissionId });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            var hasPermission = await service.HasPermissionAsync(user.UserId, "TestArea", "Read");

            // Assert
            Assert.True(hasPermission);
        }

        [Fact]
        public async Task GrantPermissionToUserAsync_OverridesRolePermission()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u2", Email = "override@example.com", IsActive = true };
            var permission = new Permission { Area = "TestArea", Name = "Delete" };
            db.Users.Add(user);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.GrantPermissionToUserAsync(user.UserId, permission.PermissionId);
            var hasPermission = await service.HasPermissionAsync(user.UserId, "TestArea", "Delete");

            // Assert
            Assert.True(hasPermission);
        }

        [Fact]
        public async Task DenyPermissionToUserAsync_BlocksPermission()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u3", Email = "deny@example.com", IsActive = true };
            var permission = new Permission { Area = "TestArea", Name = "Write" };
            db.Users.Add(user);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.DenyPermissionToUserAsync(user.UserId, permission.PermissionId);
            var hasPermission = await service.HasPermissionAsync(user.UserId, "TestArea", "Write");

            // Assert
            Assert.False(hasPermission);
        }
        [Fact]
        public async Task AssignPermissionToRoleAsync_AddsPermissionToRole()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var role = new Role { RoleName = "Editor", IsActive = true };
            var permission = new Permission { Area = "Articles", Name = "Edit" };
            db.Roles.Add(role);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.AssignPermissionToRoleAsync(role.RoleId, permission.PermissionId);

            // Assert
            var rolePermission = await db.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == permission.PermissionId);

            Assert.NotNull(rolePermission);
        }

        [Fact]
        public async Task AssignPermissionToRoleAsync_DoesNotDuplicate()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var role = new Role { RoleName = "Publisher", IsActive = true };
            var permission = new Permission { Area = "Articles", Name = "Publish" };
            db.Roles.Add(role);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.AssignPermissionToRoleAsync(role.RoleId, permission.PermissionId);
            await service.AssignPermissionToRoleAsync(role.RoleId, permission.PermissionId); // should not throw or duplicate

            // Assert
            var count = await db.RolePermissions.CountAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == permission.PermissionId);
            Assert.Equal(1, count);
        }


        [Fact]
        public async Task AssignRoleToUserAsync_AddsRoleToUser()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u100", Email = "assignrole@example.com", IsActive = true };
            var role = new Role { RoleName = "Contributor", IsActive = true };
            db.Users.Add(user);
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.AssignRoleToUserAsync(user.UserId, role.RoleId);

            // Assert
            var userRole = await db.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == user.UserId && ur.RoleId == role.RoleId);

            Assert.NotNull(userRole);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_DoesNotDuplicate()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u101", Email = "duplicateuserrole@example.com", IsActive = true };
            var role = new Role { RoleName = "Auditor", IsActive = true };
            db.Users.Add(user);
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.AssignRoleToUserAsync(user.UserId, role.RoleId);
            await service.AssignRoleToUserAsync(user.UserId, role.RoleId); // should not throw or duplicate

            // Assert
            var count = await db.UserRoles.CountAsync(ur => ur.UserId == user.UserId && ur.RoleId == role.RoleId);
            Assert.Equal(1, count);
        }


        [Fact]
        public async Task GrantPermissionToUserAsync_AddsGrantOverride()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u102", Email = "grant@example.com", IsActive = true };
            var permission = new Permission { Area = "Reports", Name = "View" };
            db.Users.Add(user);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.GrantPermissionToUserAsync(user.UserId, permission.PermissionId);

            // Assert
            var userPerm = await db.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == user.UserId && up.PermissionId == permission.PermissionId);

            Assert.NotNull(userPerm);
            Assert.Equal("GRANT", userPerm.OverrideMode);
        }

        [Fact]
        public async Task DenyPermissionToUserAsync_AddsDenyOverride()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u103", Email = "deny@example.com", IsActive = true };
            var permission = new Permission { Area = "Reports", Name = "Delete" };
            db.Users.Add(user);
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.DenyPermissionToUserAsync(user.UserId, permission.PermissionId);

            // Assert
            var userPerm = await db.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == user.UserId && up.PermissionId == permission.PermissionId);

            Assert.NotNull(userPerm);
            Assert.Equal("DENY", userPerm.OverrideMode);
        }

        [Fact]
        public async Task GrantPermissionToUserAsync_UpdatesIfExists()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u104", Email = "updategrant@example.com", IsActive = true };
            var permission = new Permission { Area = "Billing", Name = "Export" };
            db.Users.Add(user);
            db.Permissions.Add(permission);
            db.UserPermissions.Add(new UserPermission
            {
                UserId = user.UserId,
                PermissionId = permission.PermissionId,
                OverrideMode = "DENY"
            });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act
            await service.GrantPermissionToUserAsync(user.UserId, permission.PermissionId);

            // Assert
            var userPerm = await db.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == user.UserId && up.PermissionId == permission.PermissionId);

            Assert.NotNull(userPerm);
            Assert.Equal("GRANT", userPerm.OverrideMode);
        }

        [Fact]
        public async Task ChangeUserRole_RemovesOldRoleAndAddsNewRole()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var user = new User { OID = "u200", Email = "changeuserrole@example.com", IsActive = true };
            var oldRole = new Role { RoleName = "OldRole", IsActive = true };
            var newRole = new Role { RoleName = "NewRole", IsActive = true };
            db.Users.Add(user);
            db.Roles.AddRange(oldRole, newRole);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = oldRole.RoleId });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act: Remove old and add new
            var existingUserRole = await db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.UserId && ur.RoleId == oldRole.RoleId);
            if (existingUserRole != null)
            {
                db.UserRoles.Remove(existingUserRole);
                await db.SaveChangesAsync();
            }

            await service.AssignRoleToUserAsync(user.UserId, newRole.RoleId);

            // Assert
            var oldRoleExists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == oldRole.RoleId);
            var newRoleExists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == newRole.RoleId);

            Assert.False(oldRoleExists);
            Assert.True(newRoleExists);
        }

        [Fact]
        public async Task EditPermission_UpdatesNameAndArea()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var permission = new Permission { Area = "InitialArea", Name = "InitialName" };
            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            // Act
            permission.Name = "UpdatedName";
            permission.Area = "UpdatedArea";
            db.Permissions.Update(permission);
            await db.SaveChangesAsync();

            // Assert
            var updatedPermission = await db.Permissions.FirstOrDefaultAsync(p => p.PermissionId == permission.PermissionId);
            Assert.Equal("UpdatedName", updatedPermission.Name);
            Assert.Equal("UpdatedArea", updatedPermission.Area);
        }

        [Fact]
        public async Task ReplaceRolePermissions_RemovesOldPermissionsAndAddsNewOnes()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var role = new Role { RoleName = "ReplaceRole", IsActive = true };
            var perm1 = new Permission { Area = "OldArea", Name = "OldPermission" };
            var perm2 = new Permission { Area = "NewArea", Name = "NewPermission" };
            db.Roles.Add(role);
            db.Permissions.AddRange(perm1, perm2);
            await db.SaveChangesAsync();

            db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionId = perm1.PermissionId });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            // Act: Replace permission
            var oldPerm = await db.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == perm1.PermissionId);
            if (oldPerm != null)
            {
                db.RolePermissions.Remove(oldPerm);
                await db.SaveChangesAsync();
            }
            await service.AssignPermissionToRoleAsync(role.RoleId, perm2.PermissionId);

            // Assert
            var hasOld = await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == perm1.PermissionId);
            var hasNew = await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == perm2.PermissionId);
            Assert.False(hasOld);
            Assert.True(hasNew);
        }

        [Fact]
        public async Task RemoveUserPermissionOverrideAsync_RemovesOverride()
        {
            var db = GetInMemoryDbContext();
            var user = new User { OID = "test-remove", Email = "remove@example.com", IsActive = true };
            var permission = new Permission { Area = "Test", Name = "Delete" };
            db.Users.Add(user);
            db.Permissions.Add(permission);
            db.UserPermissions.Add(new UserPermission
            {
                UserId = user.UserId,
                PermissionId = permission.PermissionId,
                OverrideMode = "DENY"
            });
            await db.SaveChangesAsync();

            var service = new AuthorizationService(db);

            await service.RemoveUserPermissionOverrideAsync(user.UserId, permission.PermissionId);

            var exists = await db.UserPermissions.AnyAsync(up => up.UserId == user.UserId && up.PermissionId == permission.PermissionId);
            Assert.False(exists);
        }
    }
}


