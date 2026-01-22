using VisionPro.Core.Models;

namespace VisionPro.Core.Interfaces;

/// <summary>
/// Interface for loading and managing product configurations
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Load a product configuration from a directory
    /// </summary>
    /// <param name="productPath">Path to the product configuration folder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ProductConfig?> LoadProductConfigAsync(string productPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save a product configuration to a directory
    /// </summary>
    /// <param name="config">Product configuration to save</param>
    /// <param name="productPath">Path to save the configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> SaveProductConfigAsync(ProductConfig config, string productPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all available product configurations
    /// </summary>
    /// <param name="basePath">Base path containing product folders</param>
    Task<IEnumerable<ProductInfo>> GetAvailableProductsAsync(string basePath);
    
    /// <summary>
    /// Validate a product configuration
    /// </summary>
    /// <param name="config">Configuration to validate</param>
    ValidationResult ValidateConfig(ProductConfig config);
}

/// <summary>
/// Basic product information for listing
/// </summary>
public record ProductInfo(string ProductId, string Name, string Path, DateTime LastModified);
