using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BMS.API.Identity.Models
{
    public class PermissionsLevel
    {
        [Key]
        public int LevelId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string LevelName { get; set; }

        [Required]
        [ForeignKey("FK_PermissionsLevel_PermissionOwnerType")]
        public int PermissionOwnerType { get; set; }
    }
}
