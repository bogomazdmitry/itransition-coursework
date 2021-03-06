using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using coursework_itransition.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using coursework_itransition.Hubs;
using Identity.Models;
using ReflectionIT.Mvc.Paging;

namespace coursework_itransition
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            });

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(10);
            });
            
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddSingleton<IMailer, EmailService>();

            services.AddControllersWithViews();
            
            services.AddPaging(options => {
                options.ViewName = "Bootstrap4";
                options.PageParameterName = "pageindex";
            });
            
            services.AddRazorPages();
            services.AddSignalR();
            services.AddHttpContextAccessor();
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
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "chapter",
                    pattern: "Chapter/{action}/{compID}",
                    defaults: new { controller = "Chapter", action = "New" });
            
                endpoints.MapControllerRoute(
                    name: "chapter-edit",
                    pattern: "Chapter/{action}/{id}",
                    defaults: new { controller = "Chapter", action = "New" });
            
                endpoints.MapControllerRoute(
                    name: "composition",
                    pattern: "Composition/{action}/{id}",
                    defaults: new { controller = "Composition", action = "New" });
            
                endpoints.MapControllerRoute(
                    name: "actionwithuser",
                    pattern: "Administrator/{action}/{UserID}/{stringAction}",
                    defaults: new { controller = "Administrator", action = "ActionWithUser" });
                
                endpoints.MapControllerRoute(
                    name: "composition-admin",
                    pattern: "Composition/{action}/{UserID?}",
                    defaults: new { controller = "Composition", action = "New" });
            
                endpoints.MapControllerRoute(
                    name: "administrator",
                    pattern: "Administrator/{action}",
                    defaults: new { controller = "Administrator", action = "Administrator" });
            
                endpoints.MapControllerRoute(
                    name: "deadends",
                    pattern: "Deadends/Index/{message?}",
                    defaults: new { controller = "Deadends", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/");

                endpoints.MapControllerRoute(
                    name: "personalpage",
                    pattern: "PersonalPage/{action}/{UserID}",
                    defaults: new { controller = "PersonalPage", action = "PersonalPage" });
                    
                endpoints.MapRazorPages();
                endpoints.MapHub<CommentsHub>("/comments");
            });
        }
    }
}
