using FixFlow.Data.Seed;
using FixFlow.Services.Implementations;
using FixFlow.Services.Interfaces;
using FixFlow.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FixFlow
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<FixFlowDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<ITechnicianService, TechnicianService>();
            builder.Services.AddScoped<ISparePartService, SparePartService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<FixFlowDbContext>();

                await DbSeeder.SeedAsync(context);
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }
    }
}
