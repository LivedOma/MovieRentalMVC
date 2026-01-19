using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Movies;
using MovieRental.ViewModels.Genres;

namespace MovieRental.Controllers;

[Authorize(Roles = "Admin")]
public class GenresController : Controller
{
    private readonly ApplicationDbContext _context;

    public GenresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Genres
    public async Task<IActionResult> Index()
    {
        var genres = await _context.Genres
            .Include(g => g.MovieGenres)
            .OrderBy(g => g.Name)
            .Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name,
                MovieCount = g.MovieGenres.Count
            })
            .ToListAsync();

        return View(genres);
    }

    // GET: Genres/Create
    public IActionResult Create()
    {
        return View(new GenreViewModel());
    }

    // POST: Genres/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GenreViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // Verificar si ya existe un género con ese nombre
            var exists = await _context.Genres
                .AnyAsync(g => g.Name.ToLower() == viewModel.Name.ToLower());

            if (exists)
            {
                ModelState.AddModelError("Name", "A genre with this name already exists.");
                return View(viewModel);
            }

            var genre = new Genre
            {
                Name = viewModel.Name
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Genre '{genre.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    // GET: Genres/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var genre = await _context.Genres.FindAsync(id);

        if (genre == null) return NotFound();

        var viewModel = new GenreViewModel
        {
            GenreId = genre.GenreId,
            Name = genre.Name
        };

        return View(viewModel);
    }

    // POST: Genres/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GenreViewModel viewModel)
    {
        if (id != viewModel.GenreId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Verificar si ya existe otro género con ese nombre
                var exists = await _context.Genres
                    .AnyAsync(g => g.Name.ToLower() == viewModel.Name.ToLower() 
                                   && g.GenreId != viewModel.GenreId);

                if (exists)
                {
                    ModelState.AddModelError("Name", "A genre with this name already exists.");
                    return View(viewModel);
                }

                var genre = await _context.Genres.FindAsync(id);

                if (genre == null) return NotFound();

                genre.Name = viewModel.Name;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Genre '{genre.Name}' updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(viewModel.GenreId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    // GET: Genres/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var genre = await _context.Genres
            .Include(g => g.MovieGenres)
            .FirstOrDefaultAsync(g => g.GenreId == id);

        if (genre == null) return NotFound();

        var viewModel = new GenreViewModel
        {
            GenreId = genre.GenreId,
            Name = genre.Name,
            MovieCount = genre.MovieGenres.Count
        };

        return View(viewModel);
    }

    // POST: Genres/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.MovieGenres)
            .FirstOrDefaultAsync(g => g.GenreId == id);

        if (genre != null)
        {
            // Verificar si tiene películas asociadas
            if (genre.MovieGenres.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete genre '{genre.Name}' because it has {genre.MovieGenres.Count} movies associated.";
                return RedirectToAction(nameof(Index));
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Genre '{genre.Name}' deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool GenreExists(int id)
    {
        return _context.Genres.Any(e => e.GenreId == id);
    }
}