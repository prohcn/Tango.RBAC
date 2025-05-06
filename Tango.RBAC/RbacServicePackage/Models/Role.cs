using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.Models
{
    [Table("Role", Schema = "dbo")]
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string? RoleDescription { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveThrough { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsRetired { get; set; } = false;
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? UserUpdated { get; set; }
    }
}
