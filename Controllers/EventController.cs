using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechnoEvents.Data;
using TechnoEvents.Models;

namespace TechnoEvents.Controllers
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    @event.UserId = int.Parse(userId);

                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Profile", "User", new { id = @event.UserId });
                }
                else
                {
                    return Unauthorized();
                }
            }
            return View(@event);
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Where(e => e.IsPublic)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var @event = await _context.Events
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (@event.UserId != int.Parse(userId))
            {
                return Unauthorized();
            }

            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(Event @event)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (@event.UserId != int.Parse(userId))
                {
                    return Unauthorized();
                }

                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Events.Any(e => e.EventId == @event.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (@event.UserId != int.Parse(userId))
            {
                return Unauthorized();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> MyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var events = await _context.Events
                .Where(e => e.UserId == int.Parse(userId))
                .ToListAsync();

            return View(events);
        }
    }
}
