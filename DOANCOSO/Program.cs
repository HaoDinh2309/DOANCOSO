using DOANCOSO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
 .AddDefaultTokenProviders()
 .AddDefaultUI()
 .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

//builder.Services.AddScoped<IProductRepository, EFProductRepository>();
//builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();;
app.UseAuthorization();
app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();