using DbDocuments.Core;
using Microsoft.AspNetCore.Mvc;

namespace DbDocuments.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbDesign _design;

        public HomeController(DbDesign design)
        {
            _design = design;
        }
        public ActionResult Index()
        {
            var model = _design.GetTableInfo();
            return View(model);
        }

        public ActionResult ViewIndex()
        {
            return View(_design.GetViewInfo());
        }

        public ActionResult SPIndex()
        {
            return View(_design.GetSPInfo());
        }

    }
}
