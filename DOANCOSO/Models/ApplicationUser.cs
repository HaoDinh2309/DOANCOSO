using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DOANCOSO.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        public string? Age { get; set; }

        // Ngày tháng năm sinh
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Avatar người dùng (URL hoặc đường dẫn đến file)
        public string Avatar { get; set; }
    }
}
