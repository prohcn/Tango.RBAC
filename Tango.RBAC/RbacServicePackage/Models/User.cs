using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("User", Schema = "dbo")]
    public class User
    {
        public int UserId { get; set; }
        public string OID { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
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
