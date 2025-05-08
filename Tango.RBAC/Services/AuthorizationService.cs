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

        public async Task<bool> HasPermissionAsync(int userId, int areaTypeId, int permissionTypeId)
        {
            // Step 1: Ensure the user exists and is active
            var userIsActive = await _context.Users
                .AnyAsync(u => u.UserId == userId && u.IsActive);

            if (!userIsActive)
                return false;

            // Step 2–4: Traverse UserRoles → RolePermissions → Permissions and filter by criteria
            var hasPermission = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => _context.RolePermissions
                    .Where(rp => rp.RoleId == ur.RoleId)
                    .Select(rp => rp.PermissionId))
                .Distinct()
                .AnyAsync(pid => _context.Permissions.Any(p =>
                    p.PermissionId == pid &&
                    p.AreaTypeId == areaTypeId &&
                    p.PermissionTypeId == permissionTypeId));

            return hasPermission;
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
                user.DateCreated = DateTime.UtcNow;

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        public async Task AddRolesAsync(IEnumerable<Role> roles)
        {
            foreach (var role in roles)
                role.DateCreated = DateTime.UtcNow;

            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();
        }

        public async Task AddPermissionsAsync(IEnumerable<Permission> permissions)
        {
            foreach (var permission in permissions)
                permission.DateCreated = DateTime.UtcNow;

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId, string user)
        {
            var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (!exists)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    DateCreated = DateTime.UtcNow, 
                    UserCreated = user
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignPermissionToRoleAsync(int roleId, int permissionId, string user)
        {
            var exists = await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (!exists)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    DateCreated = DateTime.UtcNow,
                    UserCreated = user
                };
                _context.RolePermissions.Add(rolePermission);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<User?> GetUserByIdAsync(int id) => await _context.Users.FindAsync(id);
        public async Task<Role?> GetRoleByIdAsync(int id) => await _context.Roles.FindAsync(id);
        public async Task<Permission?> GetPermissionByIdAsync(int id) => await _context.Permissions.FindAsync(id);
    }
}
