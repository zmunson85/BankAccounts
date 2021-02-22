using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BankAccounts.Models;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
        public HomeController(MyContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("")]
        public IActionResult Register(Container Form)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == Form.User.Email))
                {
                    ModelState.AddModelError("User.Email", "Email already in use!");
                    return Index();
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                Form.User.Password = Hasher.HashPassword(Form.User, Form.User.Password);
                _context.Add(Form.User);
                _context.SaveChanges();

                var NewUser = _context.Users.FirstOrDefault(u => u.Email == Form.User.Email);
                int UserID = NewUser.UserId;
                HttpContext.Session.SetInt32("CurrentUser", UserID);

                return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "" });
            }
            else
            {
                return Index();
            }
        }

        [HttpGet("login")]
        public IActionResult LoginForm()
        {
            return View("Login");
        }

        [HttpPost("login")]
        public IActionResult Login(Container Form)
        {
            if (ModelState.IsValid)
            {
                User ReturningUser = _context.Users.FirstOrDefault(u => u.Email == Form.UserLogin.LoginEmail);
                if (ReturningUser == null)
                {
                    ModelState.AddModelError("UserLogin.LoginEmail", "Invalid Email Address");
                    return LoginForm();
                }
                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(Form.UserLogin, ReturningUser.Password, Form.UserLogin.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("UserLogin.LoginPassword", "Invalid Password");
                    return LoginForm();
                }
                HttpContext.Session.SetInt32("CurrentUser", ReturningUser.UserId);
                return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "" });
            }
            else
            {
                return View("Login");
            }
        }

        [HttpGet("account/{id}")]
        public IActionResult Success(int id, string error)
        {
            Container wrap = new Container();
            int? CurrentUser = HttpContext.Session.GetInt32("CurrentUser");
            User active = _context.Users.Include(u => u.Transactions).FirstOrDefault(u => u.UserId == CurrentUser);
            if (active == null)
            {
                return RedirectToAction("Index");
            }
            if (id != CurrentUser)
            {
                return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "" });
            }
            wrap.User = active;
            wrap.AllTransactions = active.Transactions.OrderByDescending(t => t.CreatedAt).ToList();
            ViewBag.Error = error;
            return View("Account", wrap);
        }

        [HttpPost("activity")]
        public IActionResult Transaction(Container Form)
        {
            if (Form.OneTransaction.Amount == 0)
            {
                return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "Amount needed to continue" });
            }
            if (ModelState.IsValid)
            {
                int? userId = HttpContext.Session.GetInt32("CurrentUser");
                User user = _context.Users.FirstOrDefault(u => u.UserId == (int)userId);
                if (user.Balance + Form.OneTransaction.Amount < 0)
                {
                    return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "Not Enough Funds To Complete Transaction" });
                }
                user.Balance += Form.OneTransaction.Amount;
                _context.Update(user);
                _context.Entry(user).Property("CreatedAt").IsModified = false;
                _context.SaveChanges();
                Form.OneTransaction.UserId = (int)userId;
                _context.Add(Form.OneTransaction);
                _context.SaveChanges();
                return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "" });
            }
            else
            {
                return RedirectToAction("Success", new { id = HttpContext.Session.GetInt32("CurrentUser"), error = "" });
            }
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}