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
        //"Injection-Apc-MultiThreaded",
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

        const double startX        = 60;
        const double startY        = 40;
        const double offsetX       = 320;
        const double offsetY       = 240;
        const int    columns       = 3;
        const double cardWidth     = 260;
        const double cardHalfH     = 80;
        const double cardHeight    = 160;

        var positions = new List<(double X, double Y)>();

        var index = 0;
        for (var current = root; current != null; current = current.Next)
        {
            var col = index % columns;
            var row = index / columns;

            var visualCol = row % 2 == 0 ? col : (columns - 1 - col);
            var x = startX + offsetX * visualCol;
            var y = startY + offsetY * row;

            positions.Add((x, y));
            Nodes.Add(new VisualizationNodeDisplay(current, x, y));
            index++;
        }

        for (int i = 0; i < positions.Count - 1; i++)
        {
            var (x1, y1) = positions[i];
            var (x2, y2) = positions[i + 1];

            Avalonia.Point start, end;

            if (Math.Abs(y1 - y2) < 1.0)
            {
                if (x2 > x1) 
                {
                    start = new Avalonia.Point(x1 + cardWidth, y1 + cardHalfH);
                    end   = new Avalonia.Point(x2,             y2 + cardHalfH);
                }
                else 
                {
                    start = new Avalonia.Point(x1,             y1 + cardHalfH);
                    end   = new Avalonia.Point(x2 + cardWidth, y2 + cardHalfH);
                }
            }
            else
            {
                var cx = x1 + cardWidth / 2.0;
                start = new Avalonia.Point(cx, y1 + cardHeight);
                end   = new Avalonia.Point(cx, y2);
            }

            Connectors.Add(new ConnectorDisplay { StartPoint = start, EndPoint = end });
        }

        CanvasWidth  = positions.Max(p => p.X) + 260 + 60;
        CanvasHeight = positions.Max(p => p.Y) + 800;
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

public sealed partial class VisualizationNodeDisplay : ObservableObject
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ZIndex))]
    private bool _isExpanded;

    public int ZIndex => _isExpanded ? 100 : 0;
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
