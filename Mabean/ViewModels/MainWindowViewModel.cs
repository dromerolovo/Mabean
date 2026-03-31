using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mabean.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public KeysManagmentViewModel KeysManagmentViewModel { get; }
    public PayloadManagerViewModel PayloadManagerViewModel { get; }
    public BehaviorSimulationViewModel BehaviorSimulationViewModel { get; }
    public EventsViewModel EventsViewModel { get; }
    public ProcessFinderViewModel ProcessFinderViewModel { get; }
    public BehaviorVisualizationViewModel BehaviorVisualizationViewModel { get; }
    public BehaviorChainViewModel BehaviorChainViewModel { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChain), nameof(ShowSimulate), nameof(ShowPayloads))]
    private int _leftTab = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEvents), nameof(ShowProcesses))]
    private int _middleTab = 0;

    public bool ShowChain => LeftTab == 0;
    public bool ShowSimulate => LeftTab == 1;
    public bool ShowPayloads => LeftTab == 2;

    public bool ShowEvents => MiddleTab == 0;
    public bool ShowProcesses => MiddleTab == 1;

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
        ProcessFinderViewModel = processFinderViewModel;
        BehaviorVisualizationViewModel = behaviorVisualizationViewModel;
        BehaviorChainViewModel = behaviorChainViewModel;

        behaviorSimulationViewModel.BrowseProcessesRequested = () => MiddleTab = 1;
        processFinderViewModel.ProcessSelected = pid =>
        {
            behaviorSimulationViewModel.Puid = pid.ToString();
            MiddleTab = 0;
        };
    }

    partial void OnLeftTabChanged(int value)
    {
        if (value == 1)
            _ = BehaviorSimulationViewModel.LoadPayloadsCommand.ExecuteAsync(null);
    }

    [RelayCommand] void SelectChain() => LeftTab = 0;
    [RelayCommand] void SelectSimulate() => LeftTab = 1;
    [RelayCommand] void SelectPayloads() => LeftTab = 2;

    [RelayCommand] void SelectEvents() => MiddleTab = 0;
    [RelayCommand] void SelectProcesses() => MiddleTab = 1;
}
