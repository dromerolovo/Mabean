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
    public partial class PayloadManagerViewModel : ViewModelBase
    {
        private readonly PayloadService _payloadService;

        [ObservableProperty]
        private string _payloadCommand = "";
        [ObservableProperty]
        private bool _hasNoPayloads = false;
        [ObservableProperty]
        private string _payloadName = "";

        [ObservableProperty]
        private ObservableCollection<string> _payloads = new();

        public PayloadManagerViewModel(PayloadService payloadService)
        {
            _payloadService = payloadService;
            _ = LoadPayloads();
        }

        [RelayCommand]
        private async Task AddPayload()
        {
            if (!string.IsNullOrWhiteSpace(PayloadCommand))
            {
                await _payloadService.AddPayload(PayloadCommand, PayloadName);
                PayloadCommand = "";
                await LoadPayloads();
            }
        }

        [RelayCommand]
        private async Task LoadPayloads()
        {
            var payloads = await _payloadService.GetPayloads();
            if (payloads != null)
            {
                Payloads = new ObservableCollection<string>(payloads);
            }
            HasNoPayloads = Payloads.Count == 0;
        }
    }
}
