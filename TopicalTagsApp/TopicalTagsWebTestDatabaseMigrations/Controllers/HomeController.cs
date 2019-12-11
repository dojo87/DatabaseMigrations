using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TopicalTagsWebTestDatabaseMigrations.Model;

namespace TopicalTagsWebTestDatabaseMigrations.Controllers
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
            return View(allTopics);
        }
    }
}
