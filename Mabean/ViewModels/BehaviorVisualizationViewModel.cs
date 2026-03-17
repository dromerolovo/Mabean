using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Mabean.ViewModels;

public partial class BehaviorVisualizationViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> _behaviors = new()
    {
        "Injection-Simple",
        "Injection-Apc-MultiThreaded",
        "Injection-Apc-EarlyBird",
        "PrivilegeEscalation-TokenTheft",
        "PrivilegeEscalation-FodHelperAbuse"
    };

    [ObservableProperty]
    private string _selectedBehavior = "Injection-Simple";
}
