using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.Models
{
    [Table("RolePermission", Schema = "dbo")]
    public class RolePermission
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
    }
}
