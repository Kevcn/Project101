using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SalonAPI.Configuration;
using SalonAPI.Helper;
using SalonAPI.Installers;
using SalonAPI.Repository;
using SalonAPI.Services;

namespace SalonAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Select all installers in Assembly.ExportedTypes
            // Assembly.ExportedTypes include Program, Startup, etc...
            // Make instance of them using Activator, and cast as IInstallers
            var installers = typeof(Startup).Assembly.ExportedTypes
                .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            installers.ForEach(installer => installer.InstallServices(services, Configuration));
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IIdentityService, IdentityService>();

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.Configure<MySqlConfig>(Configuration.GetSection("MySqlConfig"));
            services.AddOptions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // HTTP Strict Transport Security
                app.UseHsts();
            }

            // New in .NET Core 3.0 - along with app.UserEndpoints,  replaces app.UseMvc()
            // This sets up the controllers to be visible of endpoints ###!important
            // Must be declared before Authentication and Authorization for them to work ###!important
            app.UseRouting();

            app.UseHttpsRedirection();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());


            // A must for JWT authentication, obvsiouly
            app.UseAuthentication();
            // I guess this was incorporated in app.UseMvc()
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            // ******************* Swagger setup *******************
            // ******************* Swagger setup *******************
            // ******************* Swagger setup *******************

            var swaggerConfig = new SwaggerConfig();
            Configuration.GetSection(nameof(swaggerConfig)).Bind(swaggerConfig);
            app.UseSwagger(option => { option.RouteTemplate = swaggerConfig.Route; });

            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint(swaggerConfig.UiEndpoint, swaggerConfig.Description);
            });
        }
    }
}