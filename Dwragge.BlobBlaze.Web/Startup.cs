using Dwragge.BlobBlaze.Application;
using Dwragge.BlobBlaze.Application.Jobs;
using Dwragge.BlobBlaze.Storage;
using Dwragge.BlobBlaze.Web.Controllers;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz.Impl;

namespace Dwragge.BlobBlaze.Web
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
            

            services.AddMediatR();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddDataProtection();
            AddQuartz(services);
            services.AddTransient<IApplicationContextFactory, ApplicationContextFactory>();
            services.AddTransient<IDirectoryEnumerator, DirectoryEnumerator>();
            services.AddTransient<IStateRestorer, StateRestorer>();
            services.AddSingleton<IUploadProcessor, UploadProcessor>();
            services.AddSingleton<DotnetCoreJobFactory>();

            services.AddTransient<IValidator<AddFolderFormData>, AddFolderFormDataValidator>();

            services.AddTransient<DiscoverFilesJob>();
        }

        private void AddQuartz(IServiceCollection services)
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler().Result;

            services.AddSingleton(scheduler);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseMiddleware<SerilogMiddleware>();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseReactDevelopmentServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:49000");
                }
            });
        }
    }
}
