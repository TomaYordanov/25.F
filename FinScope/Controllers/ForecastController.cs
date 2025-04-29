using FinScope.Services;
using FinScope.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinScope.Controllers
{
    public class ForecastController : Controller
    {
        private readonly ForecastService _forecastService;

        public ForecastController(ForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _forecastService.GetForecastAsync();
            var model = new ForecastResultViewModel
            {
                ForecastData = data
            };
            return View(model);
        }
    }
}
