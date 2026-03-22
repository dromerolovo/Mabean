using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mabean.ViewModels;

public partial class BehaviorChainViewModel : ViewModelBase
{
    private readonly PayloadService _payloadService;
    private readonly SimulateBehaviorService _simulateBehaviorService;

    public ObservableCollection<BehaviorChainStep> Steps { get; } = [];

    public ObservableCollection<string> Behaviors { get; } = 
    [
        "Injection-Simple",
        "Injection-Apc-MultiThreaded",
        "Injection-Apc-EarlyBird",
        "PrivilegeEscalation-TokenTheft",
        "PrivilegeEscalation-FodHelperAbuse"
    ];

    private ObservableCollection<string> _payloads = [];
    public ObservableCollection<string> Payloads
    {
        get => _payloads;
        private set => SetProperty(ref _payloads, value);
    }

    public BehaviorChainViewModel(PayloadService payloadService, SimulateBehaviorService simulateBehaviorService)
    {
        _payloadService = payloadService;
        _simulateBehaviorService = simulateBehaviorService;
        AddStepCommand = new RelayCommand(AddStep);
        RemoveStepCommand = new RelayCommand<BehaviorChainStep>(RemoveStep);
        RunChainCommand = new AsyncRelayCommand(RunChain);
        _ = LoadPayloads();
    }

    public IRelayCommand AddStepCommand { get; }
    public IRelayCommand<BehaviorChainStep> RemoveStepCommand { get; }
    public IAsyncRelayCommand RunChainCommand { get; }

    private async Task LoadPayloads()
    {
        var payloads = await _payloadService.GetPayloads();
        if (payloads != null)
            Payloads = new ObservableCollection<string>(payloads);
    }

    private void AddStep()
    {
        var index = Steps.Count + 1;
        var step = new BehaviorChainStep
        {
            Name = $"Step {index}",
            Behavior = Behaviors[0],
            BehaviorOptions = Behaviors,
            PayloadOptions = Payloads
        };
        step.RemoveCommand = new RelayCommand(() => RemoveStep(step));
        Steps.Add(step);

        UpdateConnectors();
    }

    private void RemoveStep(BehaviorChainStep? step)
    {
        if (step is null) return;
        Steps.Remove(step);
        UpdateConnectors();
    }

    private async Task RunChain()
    {
        
    }

    private void UpdateConnectors()
    {
        for (var i = 0; i < Steps.Count; i++)
        {
            Steps[i].ShowConnector = i < Steps.Count - 1;
        }
    }
}

public sealed partial class BehaviorChainStep : ObservableObject
{
    private string _name = string.Empty;
    private string _behavior = string.Empty;
    private bool _showConnector;
    private IRelayCommand? _removeCommand;
    private uint _puid;
    private string _payloadName = string.Empty;
    private string _programName = string.Empty;

    public IReadOnlyList<string> BehaviorOptions { get; set; } = [];
    public IReadOnlyList<string> PayloadOptions { get; set; } = [];

    public IRelayCommand? RemoveCommand
    {
        get => _removeCommand;
        set => SetProperty(ref _removeCommand, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Behavior
    {
        get => _behavior;
        set
        {
            if (SetProperty(ref _behavior, value))
            {
                OnPropertyChanged(nameof(ShowPayloadField));
                OnPropertyChanged(nameof(ShowPuidField));
                OnPropertyChanged(nameof(ShowProgramNameField));
            }
        }
    }

    public uint Puid
    {
        get => _puid;
        set => SetProperty(ref _puid, value);
    }

    public string PayloadName
    {
        get => _payloadName;
        set => SetProperty(ref _payloadName, value);
    }

    public string ProgramName
    {
        get => _programName;
        set => SetProperty(ref _programName, value);
    }

    public bool ShowConnector
    {
        get => _showConnector;
        set => SetProperty(ref _showConnector, value);
    }

    public bool ShowPayloadField => _behavior.StartsWith("Injection");
    public bool ShowPuidField => !_behavior.Equals("Injection-Apc-EarlyBird");
    public bool ShowProgramNameField => _behavior.Equals("Injection-Apc-EarlyBird");
}
