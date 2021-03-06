using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SalonAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // using (var serviceScope = host.Services.CreateScope())
            // {
            //     var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //
            //     if (!await roleManager.RoleExistsAsync("Admin"))
            //     {
            //         var adminRole = new IdentityRole("Admin");
            //         await roleManager.CreateAsync(adminRole);
            //     }
            //
            //     if (!await roleManager.RoleExistsAsync("Visitor"))
            //     {
            //         var visitorRole = new IdentityRole("Visitor");
            //         await roleManager.CreateAsync(visitorRole);
            //     }
            // }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}