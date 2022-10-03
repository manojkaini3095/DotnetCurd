using DotnetCurd.Models;
using DotnetCurd.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetCurd
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
            //var connection = Configuration.GetConnectionString("InventoryDatabase");

            var connection = Configuration["InventoryDatabase"];
            var serviceBusConnectingString = Configuration["ServiceBusConnectionString"];
            var serviceBusTopicName = Configuration["ServiceBusTopic"];


            //services.AddSingleton<ITopicClient>(serviceProvider => new TopicClient(connectionString: Configuration.GetValue<string>("ServiceBus:ConnectionString"), entityPath: Configuration.GetValue<string>("ServiceBus:TopicName")));
            
            services.AddDbContext<InventoryContext>(options => options.UseSqlServer(connection));
            services.AddSingleton<ITopicClient>(serviceProvider => new TopicClient(connectionString: serviceBusConnectingString, entityPath: serviceBusTopicName));
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
            services.AddControllersWithViews();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Employee}/{action=Index}/{id?}");
            });
        }
    }
}
