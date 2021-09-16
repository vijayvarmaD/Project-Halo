using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMS.API.Identity.Models
{
    public class ApiResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ResourceName { get; set; }

        [Required]
        [ForeignKey("FK_ApiResource_ApiDirectory")]
        public int ApiId { get; set; }
    }
}
