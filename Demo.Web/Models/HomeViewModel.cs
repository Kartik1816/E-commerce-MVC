namespace Demo.Web.Models;

public class HomeViewModel
{
    public List<CategoryViewModel> Categories { get; set; } = null!;

    public List<ProductViewModel>? Products { get; set; }

    public List<CategoryViewModel>? FilteredCategories { get; set; }
}
