using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("Role", Schema = "dbo.rbac")]
    public class Role
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? UserUpdated { get; set; }
    }
}
