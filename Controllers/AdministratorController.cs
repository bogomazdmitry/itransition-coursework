using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using coursework_itransition.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Models;
using System.Linq;
using coursework_itransition.Models;

namespace coursework_itransition.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : Controller
    {
        public readonly ApplicationDbContext _context;
        public readonly ILogger<HomeController> _logger;
        public readonly RoleManager<IdentityRole> _roleManager;
        public readonly UserManager<ApplicationUser> _userManager;


        public AdministratorController(ApplicationDbContext context,
            ILogger<HomeController> logger,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Administrator()
        {
            return View(this);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
        // public async Task<string> RoleFind(string UserID)
        // {
        //     var u = await _context.Users.FindAsync(UserID);
        //     var lrn = await _userManager.GetRolesAsync(u);
        //     return lrn[0];
        // }

        [HttpPost, Route("Administrator/DeleteUser/{UserID?}")]
        [ValidateAntiForgeryToken]
        public IActionResult ActionWithUser(string UserID, string action)
        {
            var method = (typeof(AdministratorController)).GetMethod(action);
            string[] objects = new string[1];
            objects[0] = action;
            method.Invoke(this, objects);
            return RedirectToAction("Administrator", "Administrator");
        }

        public void DeleteUser(string UserID)
        {
            _context.Users.Remove(_context.Users.FirstOrDefault((u)=>u.Id == UserID));
            _context.SaveChanges();
            // _userManager.DeleteAsync(_context.Users.FirstOrDefault((u)=>u.Id == UserID));
        }

        public void BanUser(string UserID)
        {
            var user = _context.Users.FirstOrDefault(w=>w.Id == UserID);
            user.LockoutEnd = System.DateTime.Now.AddHours(365 * 24 * 150);
            _context.SaveChanges();
        }

        public void UnBanUser(string UserID)
        {
            var user = _context.Users.FirstOrDefault(w=>w.Id == UserID);
            user.LockoutEnd = null;
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public async void MakeAdmin(string UserID)
        {
            var user = _context.Users.FirstOrDefault(w=>w.Id == UserID);
            var listRoleUser = await _userManager.GetRolesAsync(user);
            if(listRoleUser.IndexOf("Administrator") == -1)
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
            }
        }

        public async void RemoveAdminStatus(string UserID)
        {
            var user = _context.Users.FirstOrDefault(w=>w.Id == UserID);
            var listRoleUser = await _userManager.GetRolesAsync(user);
            var num = listRoleUser.IndexOf("Administrator");
            if(num != -1)
            {
                await _userManager.RemoveFromRoleAsync(user, "Administrator");
            }
        }
    }
}
