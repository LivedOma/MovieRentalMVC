namespace MovieRental.ViewModels.Cart;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.Price);
    public int TotalItems => Items.Count;
}

public class CartItemViewModel
{
    public int CartItemId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public decimal Price { get; set; }
    public DateTime AddedAt { get; set; }
    public List<string> Genres { get; set; } = new();
}