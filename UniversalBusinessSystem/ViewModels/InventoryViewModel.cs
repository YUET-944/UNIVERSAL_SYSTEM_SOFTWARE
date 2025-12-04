using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Windows;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Services;

namespace UniversalBusinessSystem.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private List<Product> _products = new();

    [ObservableProperty]
    private List<Category> _categories = new();

    [ObservableProperty]
    private List<Unit> _units = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private Unit? _selectedUnit;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public InventoryViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize inventory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await LoadProductsAsync();
            await LoadCategoriesAsync();
            await LoadUnitsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load inventory data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadProductsAsync()
    {
        Products = (await _inventoryService.GetProductsAsync(SearchText,
                SelectedCategory?.Id,
                SelectedUnit?.Id))
            .ToList();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = (await _inventoryService.GetCategoriesAsync()).ToList();
    }

    private async Task LoadUnitsAsync()
    {
        Units = (await _inventoryService.GetUnitsAsync()).ToList();
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        try
        {
            // Create sample product for demo
            var sampleProduct = new Product
            {
                Name = $"New Product {DateTime.Now:HHmmss}",
                Sku = $"SKU-{DateTime.Now:yyMMddHHmmss}",
                CategoryId = Categories.FirstOrDefault()?.Id ?? Guid.Empty,
                UnitId = Units.FirstOrDefault()?.Id ?? Guid.Empty,
                CostPrice = 100,
                SellingPrice = 150,
                CurrentStock = 10,
                MinStockLevel = 5,
                MaxStockLevel = 100,
                IsActive = true,
                TrackStock = true,
                IsTaxable = true,
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryService.AddProductAsync(sampleProduct);

            await LoadProductsAsync();
            MessageBox.Show("Product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        try
        {
            // Create sample category for demo
            var sampleCategory = new Category
            {
                Name = $"New Category {DateTime.Now:HHmmss}",
                Description = "Sample category description",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryService.AddCategoryAsync(sampleCategory);

            await LoadCategoriesAsync();
            MessageBox.Show("Category added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditProduct(Product? product)
    {
        if (product == null) return;

        MessageBox.Show($"Edit product: {product.Name}", "Edit Product", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task DeleteProduct(Product? product)
    {
        if (product == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete '{product.Name}'?", 
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _inventoryService.DeleteProductAsync(product.Id);

                await LoadProductsAsync();
                MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void EditCategory(Category? category)
    {
        if (category == null) return;

        MessageBox.Show($"Edit category: {category.Name}", "Edit Category", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task DeleteCategory(Category? category)
    {
        if (category == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete '{category.Name}'?", 
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _inventoryService.DeleteCategoryAsync(category.Id);

                await LoadCategoriesAsync();
                MessageBox.Show("Category deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            Products = (await _inventoryService.GetProductsAsync(
                    SearchText,
                    SelectedCategory?.Id,
                    SelectedUnit?.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Search failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
