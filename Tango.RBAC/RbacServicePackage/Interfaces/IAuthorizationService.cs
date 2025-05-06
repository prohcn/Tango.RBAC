using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tango.RBAC.Models;

namespace Tango.RBAC.RbacServicePackage.Interfaces
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(int userId, string area, string name);

        Task<User> AddUserAsync(User user);
        Task<Role> AddRoleAsync(Role role);
        Task<Permission> AddPermissionAsync(Permission permission);

        Task<User> UpdateUserAsync(User user);
        Task<Role> UpdateRoleAsync(Role role);
        Task<Permission> UpdatePermissionAsync(Permission permission);

        Task DeleteUserAsync(int userId);
        Task DeleteRoleAsync(int roleId);
        Task DeletePermissionAsync(int permissionId);

        Task AddUsersAsync(IEnumerable<User> users);
        Task AddRolesAsync(IEnumerable<Role> roles);
        Task AddPermissionsAsync(IEnumerable<Permission> permissions);

        Task AssignRoleToUserAsync(int userId, int roleId, DateTime? effectiveFrom = null, DateTime? effectiveThrough = null);
        Task AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task AssignPermissionToUserAsync(int userId, int permissionId, string overrideMode);
        Task GrantPermissionToUserAsync(int userId, int permissionId);
        Task DenyPermissionToUserAsync(int userId, int permissionId);
        Task<User?> GetUserByIdAsync(int id);
        Task<Role?> GetRoleByIdAsync(int id);
        Task<Permission?> GetPermissionByIdAsync(int id);
    }
}
