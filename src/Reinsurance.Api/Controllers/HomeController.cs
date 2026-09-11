using System.Web.Mvc;

namespace Reinsurance.Api.Controllers
{
    public sealed class HomeController : Controller
    {
        /// <summary>
        /// Displays the API endpoint index.
        /// </summary>
        public ActionResult Index()
        {
            return Content("<html><body><h1>Reinsurance Workbench</h1><ul><li>GET /api/health</li><li>GET /api/submissions</li><li>GET /api/reference/perils</li><li>POST /api/treaties/{id}/price</li><li>POST /api/admin/reseed</li></ul></body></html>", "text/html");
        }
    }
}
