using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopicalTagsWeb.Models;

namespace TopicalTagsWeb.Controllers
{
    public class HomeController : Controller
    {
        private Repository Repo { get; }

        public HomeController(Repository repo)
        {
            Repo = repo;
        }

        public IActionResult Index()
        {
            var allTopics = Repo.Topics
                .Include(t => t.TopicTags)
                .ThenInclude(t => t.Tag)
                .ToList();
            return View(new Topics { All = allTopics });
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
