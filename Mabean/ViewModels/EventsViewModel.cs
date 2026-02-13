using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Abstract;
using Mabean.Models;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mabean.ViewModels
{
    public partial class EventsViewModel : ViewModelBase, IDisposable
    {
        private readonly EventsService _eventsService;
        private readonly IAiService _aiService;
        private IDisposable? _subscription;
        private readonly int _maxSecurityEvents = 200;

        [ObservableProperty]
        private ObservableCollection<SecurityEvent> _securityEvents = new();

        [ObservableProperty]
        private string _suspiciousness = string.Empty;
        [ObservableProperty]
        private string _explanation = string.Empty;

        public EventsViewModel(EventsService eventsService, IAiService aiService)
        {
            _eventsService = eventsService;
            _aiService = aiService;
            Subscribe();
        }

        private void Subscribe()
        {
            _subscription = _eventsService.EventStream
                .Buffer(TimeSpan.FromMilliseconds(100))
                .Subscribe(@event =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        foreach (var singleEvent in @event)
                        {
                            SecurityEvents.Insert(0, singleEvent);
                        }

                        while (SecurityEvents.Count > _maxSecurityEvents)
                        {
                            SecurityEvents.RemoveAt(SecurityEvents.Count - 1);
                        }
                    });
                });
        }

        [RelayCommand]
        private async Task AnalyzeSelected()
        {
            var selected = SecurityEvents.Where(e => e.IsSelected).ToList();
            if (selected.Count > 0)
            {
                var json = JsonSerializer.SerializeToNode(selected);
                var response = await _aiService.SendMessageAsync(json.ToJsonString());

                if(!string.IsNullOrEmpty(response)) {
                    try
                    {
                        response = response.Trim();
                        if (response.StartsWith("```"))
                        {
                            var firstNewline = response.IndexOf('\n');
                            if (firstNewline >= 0)
                                response = response[(firstNewline + 1)..];
                            if (response.EndsWith("```"))
                                response = response[..^3].Trim();
                        }

                        var output = JsonSerializer.Deserialize<Suspiciousness>(response);
                        Suspiciousness = output?.SuspiciousnessName ?? "Unknown";

                        Explanation = output?.Analysis ?? "No analysis available.";
                    }
                    catch (Exception ex) 
                    {
                        Suspiciousness = "Unknown";
                        Explanation = ex.Message;
                    }
                }
            }
        }

        [RelayCommand]
        private void ClearEvents() => SecurityEvents.Clear();

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
