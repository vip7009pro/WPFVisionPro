using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace VisionPro.App.ViewModels.Config;

public partial class ConditionConfigViewModel : ObservableObject
{
    [ObservableProperty]
    private string formula = "Input1 > 50 && Input2 == true";

    [ObservableProperty]
    private ObservableCollection<ConditionInputItem> inputs = new();

    public ConditionConfigViewModel()
    {
        // Sample inputs
        Inputs.Add(new ConditionInputItem { Name = "Input1", Type = "Number", Value = "0" });
        Inputs.Add(new ConditionInputItem { Name = "Input2", Type = "Boolean", Value = "False" });
    }

    [RelayCommand]
    private void AddInput()
    {
        Inputs.Add(new ConditionInputItem { Name = $"Input{Inputs.Count + 1}", Type = "Number", Value = "0" });
    }

    [RelayCommand]
    private void RemoveInput(ConditionInputItem item)
    {
        Inputs.Remove(item);
    }
}

public partial class ConditionInputItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string type = "Number"; // Number, Boolean, String

    [ObservableProperty]
    private string value = string.Empty;

    public ObservableCollection<string> AvailableTypes { get; } = new() { "Number", "Boolean", "String" };
}
