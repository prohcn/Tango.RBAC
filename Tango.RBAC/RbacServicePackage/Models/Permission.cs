using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("Permission", Schema = "dbo")]
    public class Permission
    {
        public int PermissionId { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? UserUpdated { get; set; }
    }
}
