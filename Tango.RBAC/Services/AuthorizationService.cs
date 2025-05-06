using Microsoft.EntityFrameworkCore;
using Tango.RBAC.RbacServicePackage.Models;
using Tango.RBAC.RbacServicePackage.Data;
using Tango.RBAC.RbacServicePackage.Interfaces;

namespace Tango.RBAC.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly RbacDbContext _context;

        public AuthorizationService(RbacDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int userId, string area, string name)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || !user.IsActive)
                return false;

            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            var overridePermission = userPermissions
                .FirstOrDefault(up =>
                    _context.Permissions.Any(p => p.PermissionId == up.PermissionId && p.Area == area && p.Name == name));

            if (overridePermission != null)
                return overridePermission.OverrideMode == "GRANT";

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return await _context.RolePermissions
                .Where(rp => roles.Contains(rp.RoleId))
                .AnyAsync(rp =>
                    _context.Permissions.Any(p => p.PermissionId == rp.PermissionId && p.Area == area && p.Name == name));
        }

        public async Task<User> AddUserAsync(User user)
        {
            user.DateCreated = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            role.DateCreated = DateTime.UtcNow;
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Permission> AddPermissionAsync(Permission permission)
        {
            permission.DateCreated = DateTime.UtcNow;
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            user.DateUpdated = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            role.DateUpdated = DateTime.UtcNow;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Permission> UpdatePermissionAsync(Permission permission)
        {
            permission.DateUpdated = DateTime.UtcNow;
            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePermissionAsync(int permissionId)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission != null)
            {
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddUsersAsync(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                user.DateCreated = DateTime.UtcNow;
            }
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        public async Task AddRolesAsync(IEnumerable<Role> roles)
        {
            foreach (var role in roles)
            {
                role.DateCreated = DateTime.UtcNow;
            }
            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();
        }

        public async Task AddPermissionsAsync(IEnumerable<Permission> permissions)
        {
            foreach (var permission in permissions)
            {
                permission.DateCreated = DateTime.UtcNow;
            }
            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId, DateTime? effectiveFrom = null, DateTime? effectiveThrough = null)
        {
            var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (!exists)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    EffectiveFrom = effectiveFrom,
                    EffectiveThrough = effectiveThrough,
                    DateCreated = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            var exists = await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (!exists)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    DateCreated = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task GrantPermissionToUserAsync(int userId, int permissionId)
        {
            await AddOrUpdateUserPermissionAsync(userId, permissionId, "GRANT");
        }

        public async Task DenyPermissionToUserAsync(int userId, int permissionId)
        {
            await AddOrUpdateUserPermissionAsync(userId, permissionId, "DENY");
        }

        private async Task AddOrUpdateUserPermissionAsync(int userId, int permissionId, string mode)
        {
            var userPerm = await _context.UserPermissions.FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
            if (userPerm == null)
            {
                _context.UserPermissions.Add(new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    OverrideMode = mode,
                    DateCreated = DateTime.UtcNow
                });
            }
            else
            {
                userPerm.OverrideMode = mode;
                userPerm.DateUpdated = DateTime.UtcNow;
                _context.UserPermissions.Update(userPerm);
            }
            await _context.SaveChangesAsync();
        }
        public async Task AssignPermissionToUserAsync(int userId, int permissionId, string overrideMode)
        {
            // Validate override mode
            overrideMode = overrideMode.ToUpperInvariant();
            if (overrideMode != "GRANT" && overrideMode != "DENY")
                throw new ArgumentException("OverrideMode must be either 'GRANT' or 'DENY'.");

            var existing = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (existing != null)
            {
                // Update existing override
                existing.OverrideMode = overrideMode;
                existing.DateUpdated = DateTime.UtcNow;
            }
            else
            {
                // Add new override
                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    OverrideMode = overrideMode,
                    DateCreated = DateTime.UtcNow,
                    UserCreated = "System"
                };
                _context.UserPermissions.Add(userPermission);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task RemoveUserPermissionOverrideAsync(int userId, int permissionId)
        {
            var existing = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (existing != null)
            {
                _context.UserPermissions.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
