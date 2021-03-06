using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Models;

using coursework_itransition.Models;
using coursework_itransition.Data;
using static coursework_itransition.AccessControl;

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

        [Route("Composition/New/{UserID?}")]
        public IActionResult New(string UserID = null) => View();

        [Route("Composition/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            if ((System.Object)comp != null)
            {
                if (!UserHasAccess(this.User, comp))
                    return RedirectToAction("Index", "Deadends", new { message = "You have no rights to edit this composition" });

                return View();
            }

            return RedirectToAction("Index", "Deadends", new { message = "Composition was not found." });
        }

        [HttpPost, Route("Composition/New/{UserID?}")]
        [ValidateAntiForgeryToken]
        public IActionResult New([Bind("Title,Summary,Genre")] Composition comp, string UserID = null)
        {
            var newComp = new Composition();
            newComp.CreationDT = System.DateTime.UtcNow;
            newComp.LastEditDT = System.DateTime.UtcNow;
            if(UserID == null)
                newComp.AuthorID = coursework_itransition.Utils.GetUserID(this.User);
            else
            {
                var u = this._context.Users.Find(UserID);
                if(u == null)
                    return RedirectToAction("Index", "Deadends", new { message = "User not found" });
                newComp.AuthorID = u.Id;
            }
            newComp.Title = comp.Title;
            newComp.Summary = comp.Summary;
            newComp.Genre = comp.Genre;

            _context.Add<Composition>(newComp);
            _context.SaveChanges();

            return RedirectToRoute("composition", new { controller = "Composition", action = "Edit", id = newComp.ID });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Show(string id)
        {
            if(id == null)
                return RedirectToAction("Index", "Deadends", new { message = "No ID received" });

            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .Include(c => c.Author)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            if ((System.Object)comp == null)
                return RedirectToAction("Index", "Deadends", new { message = "Composition was not found" });

            return View(comp);
        }

        [HttpPost, Route("Composition/Get")]
        public async Task<Composition> Get([FromBody] string id)
        {
            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            return comp;
        }

        [HttpPost, Route("Composition/Update")]
        public async Task<string> Update([FromBody] Composition updated)
        {
            if((System.Object)updated == null)
                return "Null received";

            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == updated.ID);

            if((System.Object)comp == null)
                return "Composition not found";


            if(!UserHasAccess(this.User, comp))
                return "You have no edit rights";

            comp.Title      = updated.Title;
            comp.Genre      = updated.Genre;
            comp.Summary    = updated.Summary;

            foreach(var chapter in comp.Chapters)
            {
                Chapter match = null;
                foreach(var c in updated.Chapters)
                    if(chapter.ID == c.ID)
                        match = c;

                if(match == null)
                    return "Bad chapters info";

                chapter.Order = match.Order;
            }

            comp.LastEditDT = System.DateTime.UtcNow;
            this._context.SaveChanges();

            return "Success";
        }

        [HttpPost, Route("Composition/Delete")]
        public async Task<string> Delete([FromBody]string id)
        {
            if (id == null)
                return "Null received";

            var comp = await this._context.Compositions
                                    .Include(c => c.Chapters)
                                    .FirstOrDefaultAsync(c => c.ID == id);

            if ((System.Object)comp == null)
                return "Composition not found";

            if(!UserHasAccess(this.User, comp))
                return "You have no rights";

            this._context.Compositions.Remove(comp);
            this._context.SaveChanges();
            return "Success";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }   
}
