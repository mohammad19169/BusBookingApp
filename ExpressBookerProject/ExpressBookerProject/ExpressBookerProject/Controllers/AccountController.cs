using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressBookerProject.Models;
using ExpressBookerProject.Utilities; // Add this using directive for the SessionManager

namespace ExpressBookerProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly expressbookerEntities _context;

        public AccountController()
        {
            _context = new expressbookerEntities();
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            if (AdminSessionManager.GetCurrentAdminSession() != null)
            {
                ViewBag.AdminLoginNotification = "Another admin is already logged in. Please try again later.";
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (AdminSessionManager.GetCurrentAdminSession() != null)
            {
                ViewBag.AdminLoginNotification = "Another admin is already logged in. Please try again later.";
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.users.FirstOrDefault(u => u.username == model.Username && u.password == model.Password);

            if (user != null)
            {
                var adminSession = new AdminSession { UserId = user.userid, Username = user.username };
                if (!AdminSessionManager.SetCurrentAdminSession(adminSession))
                {
                    ViewBag.AdminLoginNotification = "Another admin is already logged in. Please try again later.";
                    return View(model);
                }
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
        }

        public ActionResult Logout()
        {
            AdminSessionManager.ClearCurrentAdminSession();
            return RedirectToAction("Login", "Home");
        }
    }
}