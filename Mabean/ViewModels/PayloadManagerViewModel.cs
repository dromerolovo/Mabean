using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Models;
using Mabean.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mabean.ViewModels
{
    public partial class PayloadManagerViewModel : ViewModelBase
    {
        private readonly PayloadService _payloadService;
        private readonly ReverseShellService _reverseShellService;

        [ObservableProperty] private string _payloadCommand = "";
        [ObservableProperty] private string _payloadName = "";
        [ObservableProperty] private bool _hasNoPayloads = false;
        [ObservableProperty] private ObservableCollection<string> _payloads = new();

        [ObservableProperty] private string _lHost = "";
        [ObservableProperty] private string _lPort = "";
        [ObservableProperty] private ReverseShellStatus _shellStatus = ReverseShellStatus.Unconfigured;

        public bool IsUnconfigured => ShellStatus == ReverseShellStatus.Unconfigured;
        public bool IsUnavailable  => ShellStatus == ReverseShellStatus.Unavailable;
        public bool IsAvailable    => ShellStatus == ReverseShellStatus.Available;

        public PayloadManagerViewModel(PayloadService payloadService, ReverseShellService reverseShellService)
        {
            _payloadService = payloadService;
            _reverseShellService = reverseShellService;

            _reverseShellService.StatusChanged += () =>
            {
                Dispatcher.UIThread.Post(() => ShellStatus = _reverseShellService.Status);
            };

            _ = LoadPayloads();
        }

        partial void OnShellStatusChanged(ReverseShellStatus value)
        {
            OnPropertyChanged(nameof(IsUnconfigured));
            OnPropertyChanged(nameof(IsUnavailable));
            OnPropertyChanged(nameof(IsAvailable));
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

        [RelayCommand]
        private async Task ConfigureReverseShell()
        {
            if (string.IsNullOrWhiteSpace(LHost) || string.IsNullOrWhiteSpace(LPort)) return;
            await _reverseShellService.ConfigureAsync(LHost, LPort);
            await LoadPayloads();
        }
    }
}
