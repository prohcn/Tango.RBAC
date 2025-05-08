using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("UserRole", Schema = "dbo.rbac")]
    public class UserRole
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
    }
}
