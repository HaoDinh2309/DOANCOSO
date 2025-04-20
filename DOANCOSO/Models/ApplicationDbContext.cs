using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DOANCOSO.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Thêm DbSet<FlashCard> để quản lý bảng FlashCards
    public DbSet<FlashCard> FlashCards { get; set; }
}
