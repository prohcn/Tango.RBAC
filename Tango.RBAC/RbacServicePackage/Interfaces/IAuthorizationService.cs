using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tango.RBAC.RbacServicePackage.Models;


namespace Tango.RBAC.RbacServicePackage.Interfaces
{
    public interface IAuthorizationService
    {
        // Permission checking
        Task<bool> HasPermissionAsync(int userId, int areaTypeId, int permissionTypeId);

        // CRUD: User
        Task<User> AddUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<User?> GetUserByIdAsync(int id);
        Task AddUsersAsync(IEnumerable<User> users);

        // CRUD: Role
        Task<Role> AddRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(int roleId);
        Task<Role?> GetRoleByIdAsync(int id);
        Task AddRolesAsync(IEnumerable<Role> roles);

        // CRUD: Permission
        Task<Permission> AddPermissionAsync(Permission permission);
        Task<Permission> UpdatePermissionAsync(Permission permission);
        Task DeletePermissionAsync(int permissionId);
        Task<Permission?> GetPermissionByIdAsync(int id);
        Task AddPermissionsAsync(IEnumerable<Permission> permissions);

        // Assignment
        Task AssignRoleToUserAsync(int userId, int roleId, string user);
        Task AssignPermissionToRoleAsync(int roleId, int permissionId, string user);
    }
}

