using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using coursework_itransition.Models;

using Microsoft.AspNetCore.Authorization;

using coursework_itransition.Data;
using System.Security.Claims;

namespace coursework_itransition.Controllers
{
    [Authorize]
    public class CompositionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompositionController> _logger;

        public string ReturnUrl;

        [BindProperty]
        public Composition DisplayComposition { get; set; }

        public CompositionController(ApplicationDbContext context,
            ILogger<CompositionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string CurrentUserID()
        {
            var c = this.User.FindFirst(ClaimTypes.NameIdentifier);
            
            if(c == null)
                return System.String.Empty;
                
            return c.Value;
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New([Bind("Title,Summary,Genre")] Composition comp)
        {
            var newComp = new Composition();
            newComp.CreationDT = System.DateTime.UtcNow;
            newComp.LastEditDT = System.DateTime.UtcNow;
            newComp.AuthorID = coursework_itransition.Utils.GetUserID(this.User);
            newComp.Title = comp.Title;
            newComp.Summary = comp.Summary;
            newComp.Genre = comp.Genre;

            _context.Add<Composition>(newComp);
            _context.SaveChanges();

            return RedirectToRoute("default", new { controller = "Composition", action = "Edit", id = newComp.ID });
        }

        public IActionResult Edit(string id, string returnUrl = null)
        {
            var composition = _context.Compositions.Find(id);

            if ((System.Object)composition != null)
            {
                if (composition.AuthorID != CurrentUserID())
                {
                    // "you have no rights" message and back & home buttons
                    return View();
                }
            }

            ReturnUrl = new string(returnUrl);
            DisplayComposition = composition;
            return View(this);
        }

        [HttpPost]
        public IActionResult Edit(string id, string returnUrl, int ifyoudontmakethisvaritwillnotwork = 0)
        {
            var comp = this._context.Compositions.Find(id);
            if((System.Object)comp != null)
            {
                if(comp.AuthorID == CurrentUserID())
                {
                    comp.LastEditDT = System.DateTime.UtcNow;
                    comp.Title      = DisplayComposition.Title;
                    comp.Genre      = DisplayComposition.Genre;
                    comp.Summary    = DisplayComposition.Summary;

                    this._context.Compositions.Update(comp);
                    this._context.SaveChanges();

                    return Redirect(System.Web.HttpUtility.UrlDecode(returnUrl));
                }
                else
                {
                    // idk wtf do i do here
                    return Content("this aint your shit, man");
                }
            }

            return Content("didnt find any composition");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
