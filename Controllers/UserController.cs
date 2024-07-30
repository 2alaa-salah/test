using Microsoft.AspNetCore.Mvc;
using TechnoEvents.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechnoEvents.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using BCrypt.Net;

namespace TechnoEvents.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public UserController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, IFormFile imgurl)
        {
            if (imgurl != null)
            {
                var fpath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(fpath))
                {
                    Directory.CreateDirectory(fpath);
                }
                string imgPath = Path.Combine(fpath, imgurl.FileName);
                user.imgurl = imgurl.FileName;
                using (var stream = new FileStream(imgPath, FileMode.Create))
                {
                    await imgurl.CopyToAsync(stream);
                }
            }
            else
            {
                user.imgurl = "Default.png";
            }
            ModelState.Remove("imgurl");

            if (ModelState.IsValid)
            {
                
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.ConfirmPassword = user.Password; 

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and Password are required.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Profile", new { id = user.UserId });
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.Include(u => u.Events).FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || int.Parse(userId) != id)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(User user, string CurrentPassword, string NewPassword, string ConfirmNewPassword)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || int.Parse(userId) != user.UserId)
            {
                return Unauthorized();
            }

            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmNewPassword))
            {
                if (BCrypt.Net.BCrypt.Verify(CurrentPassword, existingUser.Password))
                {
                    if (NewPassword == ConfirmNewPassword)
                    {
                        existingUser.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                    }
                    else
                    {
                        ModelState.AddModelError("", "New password and confirmation do not match.");
                        return View(user);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Current password is incorrect.");
                    return View(user);
                }
            }

            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;

                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Profile));
            }
            return View(user);
        }



        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || int.Parse(userId) != id)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Event");
        }
    }
}
