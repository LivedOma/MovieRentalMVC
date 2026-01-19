using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Movies;
using MovieRental.ViewModels.MovieCredits;

namespace MovieRental.Controllers;

[Authorize(Roles = "Admin")]
public class MovieCreditsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MovieCreditsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MovieCredits/Manage/5
    public async Task<IActionResult> Manage(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies
            .Include(m => m.MovieCasts)
                .ThenInclude(mc => mc.Person)
            .Include(m => m.MovieCrews)
                .ThenInclude(mc => mc.Person)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        if (movie == null) return NotFound();

        var viewModel = new ManageCreditsViewModel
        {
            MovieId = movie.MovieId,
            MovieTitle = movie.Title,
            ReleaseYear = movie.ReleaseYear,
            Cast = movie.MovieCasts
                .OrderBy(mc => mc.CastOrder)
                .Select(mc => new CastItemViewModel
                {
                    PersonId = mc.PersonId,
                    PersonName = mc.Person.FullName,
                    CharacterName = mc.CharacterName,
                    CastOrder = mc.CastOrder
                }).ToList(),
            Crew = movie.MovieCrews
                .OrderBy(mc => mc.Role)
                .Select(mc => new CrewItemViewModel
                {
                    PersonId = mc.PersonId,
                    PersonName = mc.Person.FullName,
                    Role = mc.Role
                }).ToList()
        };

        return View(viewModel);
    }

    #region Cast Management

    // GET: MovieCredits/AddCast/5
    public async Task<IActionResult> AddCast(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null) return NotFound();

        // Obtener personas que NO están ya en el cast de esta película
        var existingCastIds = await _context.MovieCasts
            .Where(mc => mc.MovieId == id)
            .Select(mc => mc.PersonId)
            .ToListAsync();

        var availablePeople = await _context.People
            .Where(p => !existingCastIds.Contains(p.PersonId))
            .OrderBy(p => p.FullName)
            .ToListAsync();

        var viewModel = new MovieCastFormViewModel
        {
            MovieId = movie.MovieId,
            MovieTitle = movie.Title,
            CastOrder = existingCastIds.Count + 1,
            AvailablePeople = new SelectList(availablePeople, "PersonId", "FullName")
        };

        return View(viewModel);
    }

    // POST: MovieCredits/AddCast
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCast(MovieCastFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // Verificar si ya existe esta combinación
            var exists = await _context.MovieCasts
                .AnyAsync(mc => mc.MovieId == viewModel.MovieId && mc.PersonId == viewModel.PersonId);

            if (exists)
            {
                ModelState.AddModelError("PersonId", "This person is already in the cast.");
            }
            else
            {
                var movieCast = new MovieCast
                {
                    MovieId = viewModel.MovieId,
                    PersonId = viewModel.PersonId,
                    CharacterName = viewModel.CharacterName,
                    CastOrder = viewModel.CastOrder
                };

                _context.MovieCasts.Add(movieCast);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cast member added successfully!";
                return RedirectToAction(nameof(Manage), new { id = viewModel.MovieId });
            }
        }

        // Recargar datos para el formulario
        var movie = await _context.Movies.FindAsync(viewModel.MovieId);
        viewModel.MovieTitle = movie?.Title ?? "";

        var existingCastIds = await _context.MovieCasts
            .Where(mc => mc.MovieId == viewModel.MovieId)
            .Select(mc => mc.PersonId)
            .ToListAsync();

        var availablePeople = await _context.People
            .Where(p => !existingCastIds.Contains(p.PersonId))
            .OrderBy(p => p.FullName)
            .ToListAsync();

        viewModel.AvailablePeople = new SelectList(availablePeople, "PersonId", "FullName", viewModel.PersonId);

        return View(viewModel);
    }

    // GET: MovieCredits/EditCast/5/3
    public async Task<IActionResult> EditCast(int? movieId, int? personId)
    {
        if (movieId == null || personId == null) return NotFound();

        var movieCast = await _context.MovieCasts
            .Include(mc => mc.Movie)
            .Include(mc => mc.Person)
            .FirstOrDefaultAsync(mc => mc.MovieId == movieId && mc.PersonId == personId);

        if (movieCast == null) return NotFound();

        var viewModel = new MovieCastFormViewModel
        {
            MovieId = movieCast.MovieId,
            MovieTitle = movieCast.Movie.Title,
            PersonId = movieCast.PersonId,
            CharacterName = movieCast.CharacterName,
            CastOrder = movieCast.CastOrder,
            AvailablePeople = new SelectList(
                new[] { movieCast.Person }, 
                "PersonId", 
                "FullName", 
                movieCast.PersonId)
        };

        return View(viewModel);
    }

    // POST: MovieCredits/EditCast
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCast(MovieCastFormViewModel viewModel)
    {
        // Remover PersonId de validación ya que no se puede cambiar
        ModelState.Remove("PersonId");

        if (ModelState.IsValid)
        {
            var movieCast = await _context.MovieCasts
                .FirstOrDefaultAsync(mc => mc.MovieId == viewModel.MovieId && mc.PersonId == viewModel.PersonId);

            if (movieCast == null) return NotFound();

            movieCast.CharacterName = viewModel.CharacterName;
            movieCast.CastOrder = viewModel.CastOrder;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cast member updated successfully!";
            return RedirectToAction(nameof(Manage), new { id = viewModel.MovieId });
        }

        // Recargar datos
        var cast = await _context.MovieCasts
            .Include(mc => mc.Movie)
            .Include(mc => mc.Person)
            .FirstOrDefaultAsync(mc => mc.MovieId == viewModel.MovieId && mc.PersonId == viewModel.PersonId);

        if (cast != null)
        {
            viewModel.MovieTitle = cast.Movie.Title;
            viewModel.AvailablePeople = new SelectList(
                new[] { cast.Person },
                "PersonId",
                "FullName",
                viewModel.PersonId);
        }

        return View(viewModel);
    }

    // POST: MovieCredits/RemoveCast
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCast(int movieId, int personId)
    {
        var movieCast = await _context.MovieCasts
            .Include(mc => mc.Person)
            .FirstOrDefaultAsync(mc => mc.MovieId == movieId && mc.PersonId == personId);

        if (movieCast != null)
        {
            var personName = movieCast.Person.FullName;
            _context.MovieCasts.Remove(movieCast);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{personName}' removed from cast.";
        }

        return RedirectToAction(nameof(Manage), new { id = movieId });
    }

    #endregion

    #region Crew Management

    // GET: MovieCredits/AddCrew/5
    public async Task<IActionResult> AddCrew(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null) return NotFound();

        var people = await _context.People
            .OrderBy(p => p.FullName)
            .ToListAsync();

        var viewModel = new MovieCrewFormViewModel
        {
            MovieId = movie.MovieId,
            MovieTitle = movie.Title,
            AvailablePeople = new SelectList(people, "PersonId", "FullName"),
            AvailableRoles = new SelectList(MovieCrewFormViewModel.PredefinedRoles)
        };

        return View(viewModel);
    }

    // POST: MovieCredits/AddCrew
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCrew(MovieCrewFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // Verificar si ya existe esta combinación (persona + rol en esta película)
            var exists = await _context.MovieCrews
                .AnyAsync(mc => mc.MovieId == viewModel.MovieId 
                             && mc.PersonId == viewModel.PersonId 
                             && mc.Role == viewModel.Role);

            if (exists)
            {
                ModelState.AddModelError("", "This person already has this role in this movie.");
            }
            else
            {
                var movieCrew = new MovieCrew
                {
                    MovieId = viewModel.MovieId,
                    PersonId = viewModel.PersonId,
                    Role = viewModel.Role
                };

                _context.MovieCrews.Add(movieCrew);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Crew member added successfully!";
                return RedirectToAction(nameof(Manage), new { id = viewModel.MovieId });
            }
        }

        // Recargar datos para el formulario
        var movie = await _context.Movies.FindAsync(viewModel.MovieId);
        viewModel.MovieTitle = movie?.Title ?? "";

        var people = await _context.People
            .OrderBy(p => p.FullName)
            .ToListAsync();

        viewModel.AvailablePeople = new SelectList(people, "PersonId", "FullName", viewModel.PersonId);
        viewModel.AvailableRoles = new SelectList(MovieCrewFormViewModel.PredefinedRoles, viewModel.Role);

        return View(viewModel);
    }

    // POST: MovieCredits/RemoveCrew
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCrew(int movieId, int personId, string role)
    {
        var movieCrew = await _context.MovieCrews
            .Include(mc => mc.Person)
            .FirstOrDefaultAsync(mc => mc.MovieId == movieId 
                                    && mc.PersonId == personId 
                                    && mc.Role == role);

        if (movieCrew != null)
        {
            var personName = movieCrew.Person.FullName;
            _context.MovieCrews.Remove(movieCrew);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{personName}' ({role}) removed from crew.";
        }

        return RedirectToAction(nameof(Manage), new { id = movieId });
    }

    #endregion
}