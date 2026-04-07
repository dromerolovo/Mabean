using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Helpers;
using Mabean.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mabean.ViewModels
{
    public partial class ProgramFinderViewModel : ViewModelBase
    {
        private const int PageSize = 20;

        [ObservableProperty]
        private SmartObservableCollection<ProgramInfo> _programs = new();

        [ObservableProperty]
        private ObservableCollection<ProgramInfo> _pagedPrograms = new();

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

        public ProgramFinderViewModel()
        {
            IsActive = true;
        }

        public Action<string>? ProgramSelected { get; set; }

        [RelayCommand]
        private void SelectProgram(ProgramInfo program) => ProgramSelected?.Invoke(program.FullPath);

        partial void OnSearchQueryChanged(string value)
        {
            CurrentPage = 1;
            UpdatePage();
        }

        private IEnumerable<ProgramInfo> GetFiltered()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return Programs;
            var q = SearchQuery.Trim();
            return Programs.Where(p =>
                p.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdatePage()
        {
            var filtered = GetFiltered().ToList();
            TotalPages = (int)Math.Ceiling(filtered.Count / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = CurrentPage < TotalPages;
            PagedPrograms = new ObservableCollection<ProgramInfo>(
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
        private async Task LoadPrograms()
        {
            IsLoading = true;
            var programs = await Task.Run(() =>
                Directory.GetFiles(@"C:\Windows\System32")
                    .Where(f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    .Select(f => new ProgramInfo { FullPath = f, Name = Path.GetFileName(f) })
                    .OrderBy(p => p.Name)
                    .ToList());

            Programs.SuspendNotifications();
            Programs.Clear();
            foreach (var p in programs)
                Programs.Add(p);
            Programs.ResumeNotifications();
            CurrentPage = 1;
            UpdatePage();
            IsLoading = false;
        }

        protected override async void OnActivated()
        {
            await LoadPrograms();
        }
    }
}
