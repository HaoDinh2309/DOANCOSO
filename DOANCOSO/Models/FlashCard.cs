using System.ComponentModel.DataAnnotations;
namespace DOANCOSO.Models
    {
    public class FlashCard
    {
        public int Id { get; set; }

        [Required]
        public string Word { get; set; }

        [Required]
        public string Definition { get; set; }

        public string? ImageUrl { get; set; }  // Đánh dấu nullable vì đây là trường tùy chọn
    }
}

