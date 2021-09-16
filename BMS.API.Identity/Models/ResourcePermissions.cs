using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class ResourcePermissions
    {
        [Required]
        public int ResourceId { get; set; }

        [Required]
        [ForeignKey("FK_ResourcePermissions_PremissionLevel")]
        public int PremissionLevel { get; set; }

        [Required]
        public int PermissionOwnerId { get; set; }

        [Required]
        public bool MyProperty { get; set; }
    }
}
