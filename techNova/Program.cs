using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using techNova.Models;    // TechNovaDbContext
using techNova.Data;        // AuthDbContext

var builder = WebApplication.CreateBuilder(args);

// MISMA cadena de conexión para todo (ajústala a tu appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("TechNovaConnection");

// DbContext SOLO para Identity
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

// DbContext SOLO para tu sistema de tienda
builder.Services.AddDbContext<TechNovaDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity usando AuthDbContext
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
