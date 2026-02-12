using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Models
{
    public class TodoItem
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        public bool IsDone { get; set; } = false;

        /// <summary>
        /// Null represents the general list.
        /// Values 0-6 represent Sunday-Saturday (or Monday-Sunday depending on locale, usually Sunday=0 in .NET DayOfWeek).
        /// </summary>
        public DayOfWeek? Weekday { get; set; }

        /// <summary>
        /// For ordering items within their list (General or Weekday).
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public int MovedCounter { get; set; } = 0;
    }
}
