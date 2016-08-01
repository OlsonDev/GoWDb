using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Gems {
	public class Program {
		public static void Main(string[] args) {
			var cwd = Directory.GetCurrentDirectory();
			var web = Path.GetFileName(cwd) == "server" ? "../public" : "public";
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(cwd)
				.UseWebRoot(web)
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build()
			;
			host.Run();
		}
	}
}