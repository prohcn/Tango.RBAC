using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("User", Schema = "dbo.rbac")]
    public class User
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? UserUpdated { get; set; }
    }
}
