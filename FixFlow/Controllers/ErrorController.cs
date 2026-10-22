using FixFlow.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FixFlow.Controllers
{
    public class ErrorController : Controller
    {
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Index()
        {
            Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            return View(
                "~/Views/Shared/Error.cshtml",
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}