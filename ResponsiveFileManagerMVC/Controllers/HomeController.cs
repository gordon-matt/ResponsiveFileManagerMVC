using System.Web.Mvc;

namespace ResponsiveFileManagerMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IFrame()
        {
            return View();
        }

        public ActionResult Standalone()
        {
            return View();
        }
    }
}