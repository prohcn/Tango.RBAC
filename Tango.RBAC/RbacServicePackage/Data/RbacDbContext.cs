using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tango.RBAC.RbacServicePackage.Models;

namespace Tango.RBAC.RbacServicePackage.Data
{
    public class RbacDbContext : DbContext
    {
        public RbacDbContext(DbContextOptions<RbacDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<AreaType> AreaTypes => Set<AreaType>();
        public DbSet<PermissionType> PermissionTypes => Set<PermissionType>();
    }
}
