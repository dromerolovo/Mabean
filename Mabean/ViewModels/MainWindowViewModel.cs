using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mabean.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;
    public HomeViewModel HomeViewModel { get; } = new();
    public KeysManagmentViewModel KeysManagmentViewModel { get; } 
    public PayloadManagerViewModel PayloadManagerViewModel { get; } 
    public BehaviorSimulationViewModel BehaviorSimulationViewModel { get; }

    public MainWindowViewModel(KeysManagmentViewModel keysManagmentViewModel, 
        PayloadManagerViewModel payloadManagerViewModel, BehaviorSimulationViewModel behaviorSimulationViewModel)
    {
        KeysManagmentViewModel = keysManagmentViewModel;
        PayloadManagerViewModel = payloadManagerViewModel;
        BehaviorSimulationViewModel = behaviorSimulationViewModel;
        _currentPage = HomeViewModel;
    }
    public string Greeting { get; } = "Welcome to Avalonia!";

    private bool _isSidebarExpanded = true;
    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetProperty(ref _isSidebarExpanded, value);
    }

    [RelayCommand]
    public void NavigateTo(string page)
    {
        CurrentPage = page switch
        {
            "Home" => HomeViewModel,
            "KeysManagment" => KeysManagmentViewModel,
            "PayloadManager" => PayloadManagerViewModel,
            "BehaviorSimulation" => BehaviorSimulationViewModel,
            _ => CurrentPage
        };
    }
}
