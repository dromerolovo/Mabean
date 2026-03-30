using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Mabean.Views;

public partial class EventsView : UserControl
{
    public EventsView()
    {
        InitializeComponent();
        EventsGrid.AddHandler(PointerPressedEvent, OnGridPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DataGrid grid) return;

        var row = (e.Source as Avalonia.Visual)?.FindAncestorOfType<DataGridRow>();
        if (row is not null && row.DataContext == grid.SelectedItem && grid.SelectedItem != null)
        {
            grid.SelectedItem = null;
            e.Handled = true;
        }
    }
}