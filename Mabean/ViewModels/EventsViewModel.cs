using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Models;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Reactive.Linq;
using System.Text;

namespace Mabean.ViewModels
{
    public partial class EventsViewModel : ViewModelBase, IDisposable
    {
        private readonly EventsService _eventsService;
        private IDisposable? _subscription;
        private readonly int _maxSecurityEvents = 200;

        [ObservableProperty]
        private ObservableCollection<SecurityEvent> _securityEvents = new();

        public EventsViewModel(EventsService eventsService)
        {
            _eventsService = eventsService;
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
        private void ClearEvents() => SecurityEvents.Clear();

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
