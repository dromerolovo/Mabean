using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mabean.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public KeysManagmentViewModel KeysManagmentViewModel { get; }
    public PayloadManagerViewModel PayloadManagerViewModel { get; }
    public BehaviorSimulationViewModel BehaviorSimulationViewModel { get; }
    public EventsViewModel EventsViewModel { get; }
    public BehaviorVisualizationViewModel BehaviorVisualizationViewModel { get; }
    public BehaviorChainViewModel BehaviorChainViewModel { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChain), nameof(ShowSimulate), nameof(ShowPayloads))]
    private int _leftTab = 0;

    public bool ShowChain => LeftTab == 0;
    public bool ShowSimulate => LeftTab == 1;
    public bool ShowPayloads => LeftTab == 2;

    public MainWindowViewModel(
        KeysManagmentViewModel keysManagmentViewModel,
        PayloadManagerViewModel payloadManagerViewModel,
        BehaviorSimulationViewModel behaviorSimulationViewModel,
        EventsViewModel eventsViewModel,
        ProcessFinderViewModel processFinderViewModel,
        BehaviorVisualizationViewModel behaviorVisualizationViewModel,
        BehaviorChainViewModel behaviorChainViewModel)
    {
        KeysManagmentViewModel = keysManagmentViewModel;
        PayloadManagerViewModel = payloadManagerViewModel;
        BehaviorSimulationViewModel = behaviorSimulationViewModel;
        EventsViewModel = eventsViewModel;
        BehaviorVisualizationViewModel = behaviorVisualizationViewModel;
        BehaviorChainViewModel = behaviorChainViewModel;
    }

    [RelayCommand] void SelectChain() => LeftTab = 0;
    [RelayCommand] void SelectSimulate() => LeftTab = 1;
    [RelayCommand] void SelectPayloads() => LeftTab = 2;
}
