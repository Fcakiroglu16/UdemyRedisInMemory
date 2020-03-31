using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDistributedCacheRedisApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace IDistributedCacheRedisApp.Web.Controllers
{
    public class ProductsController : Controller
    {
        private IDistributedCache _distributedCache;

        public ProductsController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> Index()
        {
            DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions();

            cacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(30);

            Product product = new Product { Id = 1, Name = "Kalem", Price = 100 };

            string jsonproduct = JsonConvert.SerializeObject(product);

            Byte[] byteproduct = Encoding.UTF8.GetBytes(jsonproduct);

            _distributedCache.Set("product:1", byteproduct);

            //await _distributedCache.SetStringAsync("product:1", jsonproduct, cacheEntryOptions);

            return View();
        }

        public IActionResult Show()
        {
            Byte[] byteProduct = _distributedCache.Get("product:1");

            string jsonproduct = Encoding.UTF8.GetString(byteProduct);

            Product p = JsonConvert.DeserializeObject<Product>(jsonproduct);

            ViewBag.product = p;
            return View();
        }

        public IActionResult Remove()
        {
            _distributedCache.Remove("name");

            return View();
        }

        public IActionResult ImageUrl()
        {
            byte[] resimbyte = _distributedCache.Get("resim");

            return File(resimbyte, "image/jpg");
        }

        public IActionResult ImageCache()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/download.jpg");

            byte[] imageByte = System.IO.File.ReadAllBytes(path);

            _distributedCache.Set("resim", imageByte);

            return View();
        }
    }
}