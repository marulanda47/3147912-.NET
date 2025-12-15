using Microsoft.EntityFrameworkCore;
using Reportes.Models; // o el namespace donde quedó ChocoAdminDbContext

var builder = WebApplication.CreateBuilder(args);

// ?? DbContext para reportes
builder.Services.AddDbContext<ChocoAdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChocoAdminDb")));

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reportes}/{action=Index}/{id?}");

app.Run();
