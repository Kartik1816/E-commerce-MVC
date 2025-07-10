namespace Demo.Web.Models;

public class CategoryListViewModel
{
    public List<CategoryViewModel> Categories { get; set; } = null!;
    public CategoryViewModel? CategoryViewModel { get; set; }
}
