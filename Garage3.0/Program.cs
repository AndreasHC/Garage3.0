using Garage3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace Garage3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Build a configuration to access the connection string.
            var configuration = builder.Configuration;

            builder.Services.AddDbContext<GarageContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "Member",
                pattern: "{controller=Member}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "Vehicles",
                pattern: "{controller=Vehicles}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "VehicleTypes",
                pattern: "{controller=VehicleTypes}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
