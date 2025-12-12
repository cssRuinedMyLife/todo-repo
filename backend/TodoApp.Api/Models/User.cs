using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp.Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? GoogleSubjectId { get; set; }
    }
}
