using System.ComponentModel.DataAnnotations.Schema;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("AreaType", Schema = "dbo.rbac")]
    public class AreaType
    {
        public int AreaTypeId { get; set; }
        public required string AreaTypeName { get; set; }
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
