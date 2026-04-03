using Avalonia.Collections;
using Avalonia.Controls;
using Mabean.ViewModels;
using System.Collections.Specialized;

namespace Mabean.Views;

public partial class BehaviorVisualizationView : UserControl
{
    public BehaviorVisualizationView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is BehaviorVisualizationViewModel vm)
        {
            vm.Nodes.CollectionChanged += OnNodesChanged;
        }
    }

    private void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            NodeScrollViewer.ScrollToEnd();
    }
}
