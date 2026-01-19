using Microsoft.AspNetCore.Mvc.Rendering;
using MovieRental.Helpers;

namespace MovieRental.ViewModels.Movies;

public class MovieSearchViewModel
{
    // Filtros
    public string? SearchTerm { get; set; }
    public int? GenreId { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    // Paginación
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    // Resultados paginados
    public PaginatedList<MovieIndexViewModel>? Movies { get; set; }

    // Para los dropdowns
    public SelectList? Genres { get; set; }
    public SelectList? SortOptions { get; set; }
    public SelectList? SortOrderOptions { get; set; }
    public SelectList? PageSizeOptions { get; set; }

    // Opciones de ordenamiento
    public static List<SelectListItem> GetSortOptions() => new()
    {
        new SelectListItem { Value = "title", Text = "Title" },
        new SelectListItem { Value = "year", Text = "Release Year" },
        new SelectListItem { Value = "price", Text = "Rental Price" },
        new SelectListItem { Value = "duration", Text = "Duration" },
        new SelectListItem { Value = "created", Text = "Date Added" }
    };

    public static List<SelectListItem> GetSortOrderOptions() => new()
    {
        new SelectListItem { Value = "asc", Text = "Ascending" },
        new SelectListItem { Value = "desc", Text = "Descending" }
    };

    public static List<SelectListItem> GetPageSizeOptions() => new()
    {
        new SelectListItem { Value = "6", Text = "6 per page" },
        new SelectListItem { Value = "12", Text = "12 per page" },
        new SelectListItem { Value = "24", Text = "24 per page" },
        new SelectListItem { Value = "48", Text = "48 per page" }
    };

    // Helper para verificar si hay filtros activos
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        GenreId.HasValue ||
        YearFrom.HasValue ||
        YearTo.HasValue ||
        PriceFrom.HasValue ||
        PriceTo.HasValue;

    // Helper para construir query string preservando filtros
    public string GetQueryString(int? page = null, int? pageSize = null, string? excludeParam = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(SearchTerm) && excludeParam != "searchTerm")
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");

        if (GenreId.HasValue && excludeParam != "genreId")
            queryParams.Add($"genreId={GenreId}");

        if (YearFrom.HasValue && excludeParam != "yearFrom")
            queryParams.Add($"yearFrom={YearFrom}");

        if (YearTo.HasValue && excludeParam != "yearTo")
            queryParams.Add($"yearTo={YearTo}");

        if (PriceFrom.HasValue && excludeParam != "priceFrom")
            queryParams.Add($"priceFrom={PriceFrom}");

        if (PriceTo.HasValue && excludeParam != "priceTo")
            queryParams.Add($"priceTo={PriceTo}");

        if (!string.IsNullOrWhiteSpace(SortBy) && excludeParam != "sortBy")
            queryParams.Add($"sortBy={SortBy}");

        if (!string.IsNullOrWhiteSpace(SortOrder) && excludeParam != "sortOrder")
            queryParams.Add($"sortOrder={SortOrder}");

        var actualPage = page ?? PageIndex;
        if (actualPage > 1)
            queryParams.Add($"page={actualPage}");

        var actualPageSize = pageSize ?? PageSize;
        if (actualPageSize != 12) // Si no es el default
            queryParams.Add($"pageSize={actualPageSize}");

        return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
    }
}