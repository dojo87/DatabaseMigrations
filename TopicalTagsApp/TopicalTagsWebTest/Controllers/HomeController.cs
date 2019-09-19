using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TopicalTagsCodeMigrations.Model;

namespace TopicalTagsWebTest.Controllers
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
            var allTopics = Repo.Topics
                .Include(t => t.TopicTags)
                .ThenInclude(t => t.Tag)
                .ToList();
            return View(allTopics);
        }
    }
}
