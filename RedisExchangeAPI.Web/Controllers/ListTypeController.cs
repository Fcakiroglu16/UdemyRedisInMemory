using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;

namespace RedisExchangeAPI.Web.Controllers
{
    public class ListTypeController : Controller
    {
        private readonly RedisService _redisService;

        private readonly IDatabase db;

        private string listKey = "names";

        public ListTypeController(RedisService redisService)
        {
            _redisService = redisService;
            db = _redisService.GetDb(1);
        }

        public IActionResult Index()
        {
            List<string> namesList = new List<string>();
            if (db.KeyExists(listKey))
            {
                db.ListRange(listKey).ToList().ForEach(x =>
                {
                    namesList.Add(x.ToString());
                });
            }

            return View(namesList);
        }

        [HttpPost]
        public IActionResult Add(string name)
        {
            db.ListLeftPush(listKey, name);

            return RedirectToAction("Index");
        }

        public IActionResult DeleteItem(string name)
        {
            db.ListRemoveAsync(listKey, name).Wait();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteFirstItem()
        {
            db.ListLeftPop(listKey);
            return RedirectToAction("Index");
        }
    }
}