using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SessionApp.Web.Models;

namespace SessionApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            HttpContext.Session.SetString("name", "fatih");
            HttpContext.Session.SetString("surname", "Çakıroğlu");

            HttpContext.Session.SetInt32("total", 100);

            return View();
        }

        public IActionResult Show()
        {
            ViewBag.name = HttpContext.Session.GetString("name");

            ViewBag.surname = HttpContext.Session.GetString("surname");

            ViewBag.total = HttpContext.Session.GetInt32("total");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}