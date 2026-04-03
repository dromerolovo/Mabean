using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mabean.Builders;
using Mabean.Models;
using Mabean.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Mabean.ViewModels;

public partial class BehaviorVisualizationViewModel : ViewModelBase
{
    private readonly SimulationStepService _stepService;
    private Dictionary<string, VisualizationNode> _nodeLookup = new();

    private const double StartX     = 60;
    private const double StartY     = 40;
    private const double OffsetY    = 180;
    private const double CardWidth  = 260;
    private const double CardHeight = 80;

    private static string LibraryPath =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, "Library", "library.json"))
            ? Path.Combine(AppContext.BaseDirectory, "Library", "library.json")
            : Path.Combine(Environment.CurrentDirectory, "Library", "library.json");

    [ObservableProperty] private string _currentBehavior = "Idle";
    [ObservableProperty] private double _canvasWidth  = 420;
    [ObservableProperty] private double _canvasHeight = 600;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    private bool _isStepPending;

    [ObservableProperty]
    private bool _isBreakEnabled = true;

    partial void OnIsBreakEnabledChanged(bool value)
    {
        _stepService.IsBreakEnabled = value;
        if (!value && IsStepPending)
        {
            IsStepPending = false;
            _stepService.NextStep();
        }
    }

    public ObservableCollection<VisualizationNodeDisplay> Nodes { get; } = new();
    public ObservableCollection<ConnectorDisplay> Connectors { get; } = new();

    public BehaviorVisualizationViewModel(SimulationStepService stepService)
    {
        _stepService = stepService;
        _stepService.SimulationStarted += OnSimulationStarted;
        _stepService.StepArrived       += OnStepArrived;
    }

    private void OnSimulationStarted(string behaviorName)
    {
        CurrentBehavior = behaviorName;
        IsStepPending = false;
        Nodes.Clear();
        Connectors.Clear();
        CanvasHeight = 600;
        _nodeLookup = new VisualizationNodeBuilder().BuildLookup(LibraryPath, behaviorName);
    }

    private void OnStepArrived(string stepName, int stepIndex)
    {
        _nodeLookup.TryGetValue(stepName, out var node);
        var currentIndex = Nodes.Count;
        var y = StartY + currentIndex * OffsetY;
        Nodes.Add(new VisualizationNodeDisplay(stepName, stepIndex, node, StartX, y));

        if (currentIndex > 0)
        {
            var prevY   = StartY + (currentIndex - 1) * OffsetY;
            var centerX = StartX + CardWidth / 2.0;
            Connectors.Add(new ConnectorDisplay
            {
                StartPoint = new Point(centerX, prevY + CardHeight),
                EndPoint   = new Point(centerX, y)
            });
        }

        CanvasHeight = y + CardHeight + 120;

        if (IsBreakEnabled)
            IsStepPending = true;
    }

    [RelayCommand(CanExecute = nameof(IsStepPending))]
    private void NextStep()
    {
        IsStepPending = false;
        _stepService.NextStep();
    }
}

public sealed class VisualizationNodeDisplay
{
    public VisualizationNodeDisplay(string stepName, int stepIndex, VisualizationNode? node, double x, double y)
    {
        FunctionName = stepName;
        StepIndex    = stepIndex;
        Name         = node?.Name ?? stepName;
        Description  = node?.Description ?? string.Empty;
        X            = x;
        Y            = y;
    }

    public string FunctionName { get; }
    public string Name         { get; }
    public string Description  { get; }
    public int    StepIndex    { get; }
    public double X            { get; }
    public double Y            { get; }
}

public sealed class ConnectorDisplay
{
    public Point StartPoint { get; init; }
    public Point EndPoint   { get; init; }
}
