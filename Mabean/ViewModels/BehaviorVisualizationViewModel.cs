using CommunityToolkit.Mvvm.ComponentModel;
using Mabean.Builders;
using Mabean.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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

    private ObservableCollection<VisualizationNodeDisplay> _nodes = new();

    public ObservableCollection<VisualizationNodeDisplay> Nodes
    {
        get => _nodes;
        set => SetProperty(ref _nodes, value);
    }

    public BehaviorVisualizationViewModel()
    {
        LoadNodes();
    }

    partial void OnSelectedBehaviorChanged(string value)
    {
        LoadNodes();
    }

    private void LoadNodes()
    {
        Nodes.Clear();
        Connectors.Clear();

        var libraryPath = Path.Combine(AppContext.BaseDirectory, "Library", "library.json");
        if (!File.Exists(libraryPath))
        {
            libraryPath = Path.Combine(Environment.CurrentDirectory, "Library", "library.json");
        }

        if (!File.Exists(libraryPath))
        {
            return;
        }

        var builder = new VisualizationNodeBuilder().WithType(SelectedBehavior);
        var root = builder.Build(libraryPath);
        if (root is null)
        {
            return;
        }

        const double startX = 60;
        const double startY = 40;
        const double offsetX = 320;
        const double offsetY = 240;
        const double staggerY = 80;
        const double cardCenterX = 130;
        const double cardBottom = 160;

        var positions = new List<(double X, double Y)>();

        var index = 0;
        for (var current = root; current != null; current = current.Next)
        {
            var column = index % 2;
            var row = index / 2;
            var x = startX + offsetX * column;
            var y = startY + offsetY * row + (column == 1 ? staggerY : 0);

            positions.Add((x, y));
            Nodes.Add(new VisualizationNodeDisplay(current, x, y));
            index++;
        }

        for (int i = 0; i < positions.Count - 1; i++)
        {
            Connectors.Add(new ConnectorDisplay
            {
                StartPoint = new Avalonia.Point(positions[i].X + cardCenterX,     positions[i].Y + cardBottom),
                EndPoint   = new Avalonia.Point(positions[i + 1].X + cardCenterX, positions[i + 1].Y)
            });
        }

        CanvasWidth  = positions.Max(p => p.X) + 260 + 60;
        CanvasHeight = positions.Max(p => p.Y) + 400;
    }

    [ObservableProperty] private double _canvasWidth  = 800;
    [ObservableProperty] private double _canvasHeight = 600;

    private ObservableCollection<ConnectorDisplay> _connectors = new();
    public ObservableCollection<ConnectorDisplay> Connectors
    {
        get => _connectors;
        set => SetProperty(ref _connectors, value);
    }
}

public sealed class VisualizationNodeDisplay
{
    public VisualizationNodeDisplay(VisualizationNode node, double x, double y)
    {
        X = x;
        Y = y;
        Name = node.Name;
        Description = node.Description ?? string.Empty;
        FunctionName = node.Signature.FunctionName;
        Parameters = node.Signature.Parameters
            .Select(parameter => new VisualizationNodeParameterDisplay(parameter))
            .ToList();
    }

    public double X { get; }
    public double Y { get; }
    public string Name { get; }
    public string Description { get; }
    public string FunctionName { get; }
    public IReadOnlyList<VisualizationNodeParameterDisplay> Parameters { get; }
}

public sealed class ConnectorDisplay
{
    public Avalonia.Point StartPoint { get; init; }
    public Avalonia.Point EndPoint   { get; init; }
}

public sealed class VisualizationNodeParameterDisplay
{
    public VisualizationNodeParameterDisplay(VisualizationNodeFunctionParameter parameter)
    {
        Name = parameter.Name;
        Description = parameter.Description;
        Value = parameter.Value;
    }

    public string Name { get; }
    public string Description { get; }
    public string Value { get; }
}
