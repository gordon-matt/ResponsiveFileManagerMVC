using System.Web.Mvc;

namespace ResponsiveFileManagerMVC.ViewEngine
{
    public class PhpViewEngine : VirtualPathProviderViewEngine, IViewEngine
    {
        public PhpViewEngine()
        {
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.php" };
            base.PartialViewLocationFormats = base.ViewLocationFormats;
        }

        protected override IView CreateView(ControllerContext context, string viewPath, string masterPath)
        {
            return new PhpView(viewPath, masterPath);
        }

        protected override IView CreatePartialView(ControllerContext context, string partialPath)
        {
            return new PhpView(partialPath, "");
        }
    }
}