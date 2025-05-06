using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.Models
{
    [Table("UserPermission", Schema = "dbo")]
    public class UserPermission
    {
        public int UserPermissionId { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public string OverrideMode { get; set; }  // GRANT or DENY
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
