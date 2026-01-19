using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Movies;
using MovieRental.ViewModels.People;

namespace MovieRental.Controllers;

public class PeopleController : Controller
{
    private readonly ApplicationDbContext _context;

    public PeopleController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: People
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.People
            .Include(p => p.MovieCasts)
            .Include(p => p.MovieCrews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.FullName.ToLower().Contains(search.ToLower()));
            ViewData["Search"] = search;
        }

        var people = await query
            .OrderBy(p => p.FullName)
            .Select(p => new PersonIndexViewModel
            {
                PersonId = p.PersonId,
                FullName = p.FullName,
                BirthDate = p.BirthDate,
                MoviesAsActor = p.MovieCasts.Count,
                MoviesAsCrew = p.MovieCrews.Count
            })
            .ToListAsync();

        return View(people);
    }

    // GET: People/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var person = await _context.People
            .Include(p => p.MovieCasts)
                .ThenInclude(mc => mc.Movie)
            .Include(p => p.MovieCrews)
                .ThenInclude(mc => mc.Movie)
            .FirstOrDefaultAsync(p => p.PersonId == id);

        if (person == null) return NotFound();

        var viewModel = new PersonDetailsViewModel
        {
            PersonId = person.PersonId,
            FullName = person.FullName,
            BirthDate = person.BirthDate,
            Bio = person.Bio,
            MoviesAsActor = person.MovieCasts
                .OrderByDescending(mc => mc.Movie.ReleaseYear)
                .Select(mc => new MovieRoleViewModel
                {
                    MovieId = mc.MovieId,
                    MovieTitle = mc.Movie.Title,
                    ReleaseYear = mc.Movie.ReleaseYear,
                    CharacterName = mc.CharacterName,
                    CastOrder = mc.CastOrder
                }).ToList(),
            MoviesAsCrew = person.MovieCrews
                .OrderByDescending(mc => mc.Movie.ReleaseYear)
                .Select(mc => new MovieCrewRoleViewModel
                {
                    MovieId = mc.MovieId,
                    MovieTitle = mc.Movie.Title,
                    ReleaseYear = mc.Movie.ReleaseYear,
                    Role = mc.Role
                }).ToList()
        };

        return View(viewModel);
    }

    // GET: People/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new PersonFormViewModel());
    }

    // POST: People/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(PersonFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var person = new Person
            {
                FullName = viewModel.FullName,
                BirthDate = viewModel.BirthDate,
                Bio = viewModel.Bio
            };

            _context.People.Add(person);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Person '{person.FullName}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    // GET: People/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var person = await _context.People.FindAsync(id);

        if (person == null) return NotFound();

        var viewModel = new PersonFormViewModel
        {
            PersonId = person.PersonId,
            FullName = person.FullName,
            BirthDate = person.BirthDate,
            Bio = person.Bio
        };

        return View(viewModel);
    }

    // POST: People/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, PersonFormViewModel viewModel)
    {
        if (id != viewModel.PersonId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var person = await _context.People.FindAsync(id);

                if (person == null) return NotFound();

                person.FullName = viewModel.FullName;
                person.BirthDate = viewModel.BirthDate;
                person.Bio = viewModel.Bio;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Person '{person.FullName}' updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(viewModel.PersonId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    // GET: People/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var person = await _context.People
            .Include(p => p.MovieCasts)
            .Include(p => p.MovieCrews)
            .FirstOrDefaultAsync(p => p.PersonId == id);

        if (person == null) return NotFound();

        var viewModel = new PersonDetailsViewModel
        {
            PersonId = person.PersonId,
            FullName = person.FullName,
            BirthDate = person.BirthDate,
            MoviesAsActor = person.MovieCasts.Select(mc => new MovieRoleViewModel
            {
                MovieTitle = mc.Movie?.Title ?? "Unknown"
            }).ToList(),
            MoviesAsCrew = person.MovieCrews.Select(mc => new MovieCrewRoleViewModel
            {
                MovieTitle = mc.Movie?.Title ?? "Unknown"
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: People/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var person = await _context.People
            .Include(p => p.MovieCasts)
            .Include(p => p.MovieCrews)
            .FirstOrDefaultAsync(p => p.PersonId == id);

        if (person != null)
        {
            // Verificar si tiene películas asociadas
            if (person.MovieCasts.Any() || person.MovieCrews.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{person.FullName}' because they have movies associated.";
                return RedirectToAction(nameof(Index));
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Person '{person.FullName}' deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PersonExists(int id)
    {
        return _context.People.Any(e => e.PersonId == id);
    }
}