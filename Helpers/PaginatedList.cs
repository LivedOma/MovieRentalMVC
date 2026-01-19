using Microsoft.EntityFrameworkCore;

namespace MovieRental.Helpers;

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }
    public int PageSize { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalCount = count;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public int FirstItemIndex => (PageIndex - 1) * PageSize + 1;
    public int LastItemIndex => Math.Min(PageIndex * PageSize, TotalCount);

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    // Para usar con listas ya materializadas
    public static PaginatedList<T> Create(
        List<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count;
        var items = source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    // Genera los números de página a mostrar
    public IEnumerable<int> GetPageNumbers(int maxPagesToShow = 5)
    {
        int startPage = Math.Max(1, PageIndex - maxPagesToShow / 2);
        int endPage = Math.Min(TotalPages, startPage + maxPagesToShow - 1);

        // Ajustar si estamos cerca del final
        if (endPage - startPage + 1 < maxPagesToShow)
        {
            startPage = Math.Max(1, endPage - maxPagesToShow + 1);
        }

        return Enumerable.Range(startPage, endPage - startPage + 1);
    }
}