using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mabean.Services;

public class SimulationStepService
{
    private readonly SemaphoreSlim _gate = new(0, 1);
    private StepCallbackDelegate? _callbackRef;

    public bool IsBreakEnabled { get; set; } = true;

    public event Action<string>? SimulationStarted;
    public event Action<string, int>? StepArrived;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StepCallbackDelegate(
        [MarshalAs(UnmanagedType.LPStr)] string stepName,
        int stepIndex);

    public StepCallbackDelegate GetCallback()
    {
        _callbackRef = OnStep;
        return _callbackRef;
    }

    public void BeginSimulation(string behaviorName)
    {
        Dispatcher.UIThread.Post(() => SimulationStarted?.Invoke(behaviorName));
    }

    private void OnStep(string stepName, int stepIndex)
    {
        Dispatcher.UIThread.Post(() => StepArrived?.Invoke(stepName, stepIndex));
        if (IsBreakEnabled)
            _gate.Wait();
    }

    public void NextStep()
    {
        if (_gate.CurrentCount == 0)
            _gate.Release();
    }
}
