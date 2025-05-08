using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tango.RBAC.RbacServicePackage.Models
{
    [Table("PermissionType", Schema = "dbo.rbac")]
    public class PermissionType
    {
        public int PermissionTypeId { get; set; }
        public required string PermissionTypeName { get; set; }
        public DateTime DateCreated { get; set; }
        public string? UserCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}