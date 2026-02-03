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
        private ObservableCollection<string> _payloads = new ObservableCollection<string>()
        {
            "Calc"
        };

        [ObservableProperty]
        private ObservableCollection<string> _behaviors = new ObservableCollection<string>()
        {
            "Injection-Simple"
        };

        [ObservableProperty]
        private string _selectedBehavior = "";

        [ObservableProperty]
        private string _selectedPayload = "";
        [ObservableProperty]
        private uint _puid = default;
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
            LoggerService.Write($"[+] Executing behavior: {SelectedBehavior} into process with PUID: {Puid} using payload: {SelectedPayload}");
            await _simulateBehaviorService.InjectBehavior(Puid, SelectedBehavior, SelectedPayload);
        }
    }
}
