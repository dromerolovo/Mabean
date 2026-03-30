using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Helpers;
using Mabean.Models;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mabean.ViewModels
{
    public partial class ProcessFinderViewModel : ViewModelBase
    {
        private readonly ProcessFinderService _processFinderService;
        private const int PageSize = 20;

        [ObservableProperty]
        private SmartObservableCollection<ProcessInfo> _processes = new();

        [ObservableProperty]
        private ObservableCollection<ProcessInfo> _pagedProcesses = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private bool _hasPreviousPage;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private string _searchQuery = "";

        public Action<int>? ProcessSelected { get; set; }

        partial void OnSearchQueryChanged(string value)
        {
            CurrentPage = 1;
            UpdatePage();
        }

        public ProcessFinderViewModel(ProcessFinderService processFinderService)
        {
            _processFinderService = processFinderService;
            IsActive = true;
        }

        [RelayCommand]
        private void SelectProcess(ProcessInfo process) => ProcessSelected?.Invoke(process.ProcessId);

        private IEnumerable<ProcessInfo> GetFiltered()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return Processes;
            var q = SearchQuery.Trim();
            return Processes.Where(p =>
                p.ProcessName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.ProcessId.ToString().Contains(q) ||
                p.UserName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdatePage()
        {
            var filtered = GetFiltered().ToList();
            TotalPages = (int)Math.Ceiling(filtered.Count / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = CurrentPage < TotalPages;
            PagedProcesses = new ObservableCollection<ProcessInfo>(
                filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
        }

        [RelayCommand]
        private void NextPage()
        {
            CurrentPage++;
            UpdatePage();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            CurrentPage--;
            UpdatePage();
        }

        [RelayCommand]
        private async Task LoadProcesses()
        {
            IsLoading = true;
            var processes = await Task.Run(() => _processFinderService.GetProcesses());
            Processes.SuspendNotifications();
            Processes.Clear();
            foreach (var p in processes)
                Processes.Add(p);
            Processes.ResumeNotifications();
            CurrentPage = 1;
            UpdatePage();
            IsLoading = false;
        }

        protected override async void OnActivated()
        {
            await LoadProcesses();
        }
    }
}
