namespace VisionPro.App.Services;

/// <summary>
/// Navigation service interface
/// </summary>
public interface INavigationService
{
    void NavigateToTab(int tabIndex);
    int CurrentTab { get; }
}

/// <summary>
/// Simple navigation service for tab-based navigation
/// </summary>
public class NavigationService : INavigationService
{
    private readonly ViewModels.MainViewModel _mainViewModel;
    
    public NavigationService(ViewModels.MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }
    
    public int CurrentTab => _mainViewModel.SelectedTabIndex;
    
    public void NavigateToTab(int tabIndex)
    {
        _mainViewModel.SelectedTabIndex = tabIndex;
    }
}
