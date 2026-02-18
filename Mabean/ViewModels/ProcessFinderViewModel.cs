using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Models;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.ViewModels
{
    public partial class ProcessFinderViewModel : ViewModelBase
    {
        private readonly ProcessFinderService _processFinderService;

        [ObservableProperty]
        private ObservableCollection<ProcessInfo> _processes = new();

        [ObservableProperty]
        private bool _isLoading;
        public ProcessFinderViewModel(ProcessFinderService processFinderService)
        {
            _processFinderService = processFinderService;
            IsActive = true;
        }

        [RelayCommand]
        private async Task LoadProcesses()
        {
            IsLoading = true;
            var processes = await Task.Run(() => _processFinderService.GetProcesses());
            Processes = new ObservableCollection<ProcessInfo>(processes);
            IsLoading = false;
        }

        protected override async void OnActivated()
        {
            await LoadProcesses();
        }

    }
}
