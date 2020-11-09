using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using coursework_itransition.Models;

using Microsoft.AspNetCore.Authorization;

using coursework_itransition.Data;

using Microsoft.AspNetCore.Identity;
using Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace coursework_itransition.Controllers
{
    [Authorize]
    public class CompositionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompositionController> _logger;
        public readonly UserManager<ApplicationUser> _userManager;


        public CompositionController(ApplicationDbContext context,
            ILogger<CompositionController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult New() => View();
        public IActionResult NoEditRights() => View();
        public IActionResult CompNotFound() => View();

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

            return RedirectToRoute("composition", new { controller = "Composition", action = "Edit", id = newComp.ID });
        }

        public async Task<IActionResult> Edit(string id, string returnUrl)
        {
            var composition = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            if ((System.Object)composition != null)
            {
                if (!(coursework_itransition.Utils.UserIsAuthor(this.User, composition) || this.User.IsInRole("Administrator")))
                    return RedirectToAction("NoEditRights");

                return View(composition);
            }

            return RedirectToAction("CompNotFound");
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(string id, string returnUrl, [Bind("Title,Summary,Genre")] Composition editedComp)
        {
            var comp = this._context.Compositions.Find(id);
            if((System.Object)comp != null)
            {
                if(coursework_itransition.Utils.UserIsAuthor(this.User, comp) || this.User.IsInRole("Administrator"))
                {
                    comp.LastEditDT = System.DateTime.UtcNow;
                    comp.Title      = editedComp.Title;
                    comp.Genre      = editedComp.Genre;
                    comp.Summary    = editedComp.Summary;

                    this._context.Compositions.Update(comp);
                    this._context.SaveChanges();

                    if(returnUrl == null)
                        return Redirect("~/");
                    return Redirect(System.Web.HttpUtility.UrlDecode(returnUrl));
                }
                else
                {
                    return RedirectToAction("NoEditRights");
                }
            }

            return RedirectToAction("CompNotFound");
        }

        [HttpPost, Route("Composition/DeleteComp")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComp(string id, string returnUrl = null)
        {
            if(id == null)
                return RedirectToAction("CompNotFound");

            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            if((System.Object)comp == null)
                return RedirectToAction("CompNotFound");

            if(!coursework_itransition.Utils.UserIsAuthor(this.User, comp))
                return RedirectToAction("NoEditRights");

            // check if deleted
            this._context.Compositions.Remove(comp);
            this._context.SaveChanges();

            if (returnUrl == null)
                return Redirect("~/");
            return Redirect(System.Web.HttpUtility.UrlDecode(returnUrl));
        }

        public async Task<IActionResult> Show(string id)
        {
            if(id == null)
                return RedirectToAction("CompNotFound");

            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            if ((System.Object)comp == null)
                return RedirectToAction("CompNotFound");

            return View(comp);
        }

//////////////////////////////////////////////////////////////////////////////////////////////

        private bool UpdateChapter(Chapter updated)
        {
            if((System.Object)updated == null)
                return false;

            var chapter = this._context.Chapters.Find(updated.ID);

            if((System.Object)chapter == null)
                return false;

            chapter.Title = updated.Title;
            chapter.Order = updated.Order;
            chapter.Contents = updated.Contents;
            chapter.LastEditDT = System.DateTime.UtcNow;

            this._context.SaveChanges();
            return true;
        }

        [HttpPost, Route("Composition/GetComposition")]
        public async Task<Composition> GetComposition([FromBody] string id)
        {
            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            return comp;
        }

        [HttpPost, Route("Composition/UpdateComposition")]
        public async Task<string> UpdateComposition([FromBody] Composition updated)
        {
            if((System.Object)updated == null)
                return "Null received";

            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == updated.ID);

            if((System.Object)comp == null)
                return "Composition not found";

            comp.Title = updated.Title;
            comp.Genre = updated.Genre;
            comp.Summary = updated.Summary;

            foreach(var chapter in updated.Chapters)
                UpdateChapter(chapter);

            comp.LastEditDT = System.DateTime.UtcNow;

            return "Success";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }   
}
