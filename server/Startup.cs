using Gems.Conventions;
using Gems.Models.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Gems.Services;

namespace Gems {
	public class Startup {
		public IConfigurationRoot Configuration { get; set; }

		public Startup(IHostingEnvironment env) {
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
			;

			if (env.IsDevelopment()) {
				builder.AddUserSecrets();
				// This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
				builder.AddApplicationInsightsSettings(developerMode: true);
			}
			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public void ConfigureServices(IServiceCollection services) {
			services.AddApplicationInsightsTelemetry(Configuration);

			services.AddEntityFramework()
				.AddEntityFrameworkSqlServer()
				.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")))
			;
			services.AddMemoryCache();
			services.AddSession(options => {
				options.IdleTimeout = TimeSpan.FromDays(14);
			});

			services.AddMvc(options => {
				// options.Conventions.Add(new HyphenatedRoutingConvention());
			});

			services.AddSingleton<IConfiguration>(sp => Configuration);
			services.AddSingleton<GameDataDecrypterService>();

			// These should be request scoped so DbContext leases a new connection and doesn't close it on other requests
			//services
			//	.AddScoped<KingdomService>()
			//	.AddScoped<TroopService>()
			//;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			app.UseApplicationInsightsRequestTelemetry();

			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
				app.UseBrowserLink();
			} else {
				app.UseExceptionHandler("/error");
				try {
					using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
						serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
					}
				} catch {
					// TODO: Should this be handled? Snippet copy-pasted from Microsoft example
					// http://docs.asp.net/en/latest/conceptual-overview/understanding-aspnet5-apps.html
				}
			}
			app.UseApplicationInsightsExceptionTelemetry();
			app.UseStaticFiles();
			app.UseSession();
			app.UseMvc(routes => {
				routes.MapRoute(
					name: "masters",
					template: "{path:regex(kingdoms|troops|weapons|classes|quests|admin)}",
					defaults: new { controller = "Home", action = "Index" }
				);
				routes.MapRoute(
					name: "details",
					template: "{path:regex(kingdom|troop|weapon|class|quest)}/{id}",
					defaults: new { controller = "Home", action = "Index" }
				);
				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}