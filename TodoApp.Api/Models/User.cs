using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? GoogleSubjectId { get; set; }
    }
}
