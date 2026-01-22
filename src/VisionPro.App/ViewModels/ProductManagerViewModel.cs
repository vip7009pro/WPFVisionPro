using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;

namespace VisionPro.App.ViewModels;

/// <summary>
/// ViewModel for Product Manager screen
/// </summary>
public partial class ProductManagerViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ProductInfo> products = new();
    
    [ObservableProperty]
    private ProductInfo? selectedProduct;
    
    [ObservableProperty]
    private ProductConfig? currentConfig;
    
    [ObservableProperty]
    private string productsPath = @".\config\Products";
    
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        Products.Clear();
        
        if (!Directory.Exists(ProductsPath))
            return;
        
        await Task.Run(() =>
        {
            foreach (var dir in Directory.GetDirectories(ProductsPath))
            {
                var productJsonPath = Path.Combine(dir, "product.json");
                if (File.Exists(productJsonPath))
                {
                    try
                    {
                        var json = File.ReadAllText(productJsonPath);
                        var product = System.Text.Json.JsonSerializer.Deserialize<ProductDefinition>(json);
                        
                        if (product != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                Products.Add(new ProductInfo(
                                    product.ProductId,
                                    product.ProductName,
                                    dir,
                                    Directory.GetLastWriteTime(dir)));
                            });
                        }
                    }
                    catch
                    {
                        // Skip invalid product configs
                    }
                }
            }
        });
    }
    
    [RelayCommand]
    private void LoadSelectedProduct()
    {
        if (SelectedProduct == null)
            return;
        
        // TODO: Load full product configuration
    }
    
    [RelayCommand]
    private void CreateNewProduct()
    {
        // TODO: Create new product wizard
    }
    
    [RelayCommand]
    private void DeleteSelectedProduct()
    {
        if (SelectedProduct == null)
            return;
        
        // TODO: Confirm and delete product
    }
}
