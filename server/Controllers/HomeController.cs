using Microsoft.AspNetCore.Mvc;

namespace Gems.Controllers {
	public class HomeController : BaseController {
		public IActionResult Index() {
			ViewData["Title"] = "Home";
			return View();
		}
	}
}