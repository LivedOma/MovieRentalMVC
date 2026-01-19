using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Users;
using MovieRental.ViewModels.Cart;

namespace MovieRental.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Cart
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var cartItems = await _context.CartItems
            .Include(c => c.Movie)
                .ThenInclude(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.AddedAt)
            .Select(c => new CartItemViewModel
            {
                CartItemId = c.CartItemId,
                MovieId = c.MovieId,
                MovieTitle = c.Movie.Title,
                ReleaseYear = c.Movie.ReleaseYear,
                Price = c.PriceAtAddition,
                AddedAt = c.AddedAt,
                Genres = c.Movie.MovieGenres.Select(mg => mg.Genre.Name).ToList()
            })
            .ToListAsync();

        var viewModel = new CartViewModel
        {
            Items = cartItems
        };

        return View(viewModel);
    }

    // POST: Cart/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Verificar si la película existe
        var movie = await _context.Movies.FindAsync(model.MovieId);

        if (movie == null)
        {
            TempData["ErrorMessage"] = "Movie not found.";
            return RedirectToLocal(model.ReturnUrl);
        }

        // Verificar si ya está en el carrito
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == model.MovieId);

        if (existingItem != null)
        {
            TempData["WarningMessage"] = $"'{movie.Title}' is already in your cart.";
            return RedirectToLocal(model.ReturnUrl);
        }

        // Agregar al carrito
        var cartItem = new CartItem
        {
            UserId = userId,
            MovieId = model.MovieId,
            PriceAtAddition = movie.RentalPrice,
            AddedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"'{movie.Title}' has been added to your cart.";

        return RedirectToLocal(model.ReturnUrl);
    }

    // POST: Cart/Remove/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = _userManager.GetUserId(User);

        var cartItem = await _context.CartItems
            .Include(c => c.Movie)
            .FirstOrDefaultAsync(c => c.CartItemId == id && c.UserId == userId);

        if (cartItem != null)
        {
            var movieTitle = cartItem.Movie.Title;
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{movieTitle}' has been removed from your cart.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var userId = _userManager.GetUserId(User);

        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Any())
        {
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your cart has been cleared.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Cart/Checkout
    public async Task<IActionResult> Checkout()
    {
        var userId = _userManager.GetUserId(User);

        var cartItems = await _context.CartItems
            .Include(c => c.Movie)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["WarningMessage"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new CartViewModel
        {
            Items = cartItems.Select(c => new CartItemViewModel
            {
                CartItemId = c.CartItemId,
                MovieId = c.MovieId,
                MovieTitle = c.Movie.Title,
                ReleaseYear = c.Movie.ReleaseYear,
                Price = c.PriceAtAddition,
                AddedAt = c.AddedAt
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: Cart/ProcessCheckout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessCheckout()
    {
        var userId = _userManager.GetUserId(User);

        var cartItems = await _context.CartItems
            .Include(c => c.Movie)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["WarningMessage"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        // Aquí normalmente procesarías el pago
        // Por ahora, solo simulamos el checkout

        var totalAmount = cartItems.Sum(c => c.PriceAtAddition);
        var movieCount = cartItems.Count;

        // Limpiar el carrito después del checkout
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Checkout successful! You rented {movieCount} movie(s) for {totalAmount:C}. Enjoy!";

        return RedirectToAction("Index", "Movies");
    }

    // API: Obtener conteo del carrito (para navbar)
    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var userId = _userManager.GetUserId(User);

        var count = await _context.CartItems
            .CountAsync(c => c.UserId == userId);

        return Json(new { count });
    }

    // Helper method
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Movies");
    }
}