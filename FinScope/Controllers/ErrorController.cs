using FinScope.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinScope.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleStatusCode(int statusCode)
        {
            var viewModel = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };

            return View($"Error{statusCode}", viewModel);
        }

        [Route("Error")]
        public IActionResult HandleException()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var viewModel = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = 500
            };

            return View("Error500", viewModel);
        }
    }
}
