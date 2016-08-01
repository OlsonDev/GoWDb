using Microsoft.AspNetCore.Mvc;

namespace Gems.Controllers {
	public class ErrorController : BaseController {
		public IActionResult Index() {
			ViewData["Title"] = "Error";
			return View();
		}
	}
}