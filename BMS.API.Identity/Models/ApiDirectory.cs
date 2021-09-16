using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class ApiDirectory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApiName { get; set; }
    }
}
