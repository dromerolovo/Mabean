using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.ViewModels
{
    public partial class BehaviorSimulationViewModel : ViewModelBase
    {

        private readonly PayloadService _payloadService;
        private readonly SimulateBehaviorService _simulateBehaviorService;

        [ObservableProperty]
        private ObservableCollection<string> _payloads = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> _behaviors = new ObservableCollection<string>()
        {
            "Injection-Simple",
            "Injection-Apc-MultiThreaded",
            "Injection-Apc-EarlyBird",
            "PrivilegeEscalation-TokenTheft"
        };

        [ObservableProperty]
        private string _selectedBehavior = "Injection-Simple";

        [ObservableProperty]
        private string _selectedPayload = "";
        [ObservableProperty]
        private uint _puid = default;

        [ObservableProperty]
        private string _programName = "";

        public bool ShowPayloadField => SelectedBehavior.StartsWith("Injection");
        public bool ShowProgramNameField => SelectedBehavior.Equals("Injection-Apc-EarlyBird");
        public bool ShowPuidField => !SelectedBehavior.Equals("Injection-Apc-EarlyBird");

        public BehaviorSimulationViewModel(PayloadService payloadService, SimulateBehaviorService simulateBehaviorService)
        {
            _payloadService = payloadService;
            _ = LoadPayloads();
            _simulateBehaviorService = simulateBehaviorService;
        }

        [RelayCommand]
        private async Task LoadPayloads()
        {
            var payloads = await _payloadService.GetPayloads();
            if (payloads != null)
            {
                Payloads = new ObservableCollection<string>(payloads);
            }
        }

        [RelayCommand]
        private async Task ExecuteBehavior()
        {
            Console.WriteLine(ProgramName);
            LoggerService.Write($"[+] Executing behavior: {SelectedBehavior} into process with PUID: {Puid} using payload: {SelectedPayload}");
            await _simulateBehaviorService.InjectBehavior(Puid, SelectedBehavior, SelectedPayload, ProgramName);
        }

        partial void OnSelectedBehaviorChanged(string value)
        {
            OnPropertyChanged(nameof(ShowPuidField));
            OnPropertyChanged(nameof(ShowProgramNameField));
            OnPropertyChanged(nameof(ShowPayloadField));
        }
    }
}
