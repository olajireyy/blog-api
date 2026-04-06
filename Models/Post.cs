using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? UserId { get; set; }
    }
}