using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TopicalTags.WebTest.DatabaseFirstTransitions.Model;

namespace TopicalTags.WebTest.DatabaseFirstTransitions.Controllers
{
    public class HomeController : Controller
    {
        private TopicContext Repo { get; }

        public HomeController(TopicContext repo)
        {
            Repo = repo;
        }

        public IActionResult Index()
        {
            var allTopics = Repo.Topic
                .Include(t => t.TopicTags)
                .ThenInclude(t => t.Tag)
                .ToList();

            ViewData["Configuration"] = string.Join(" | ", Repo.Configuration.Select(c => $"{c.Key}={c.Value}"));

            return View(allTopics);
        }
    }
}
