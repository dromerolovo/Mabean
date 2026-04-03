using Mabean.Helpers;
using Mabean.Services;
using System.IO;
using System.Runtime.InteropServices;

namespace Mabean.Interop
{
    internal static class InteropInjection
    {
        public static void Init()
        {
            string dllInjection = Path.Combine(Paths.Dlls, "1.dll");
            NativeLibrary.Load(dllInjection);
        }

        [DllImport("1.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        public static extern int InjectPayloadSimple(
            uint pid,
            byte[] payload,
            uint length,
            SimulationStepService.StepCallbackDelegate? callback
        );

        [DllImport("1.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        public static extern int InjectPayloadApcMultiThreaded(
            uint pid,
            byte[] payload,
            nuint length,
            SimulationStepService.StepCallbackDelegate? callback
        );

        [DllImport("1.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        public static extern int InjectPayloadApcEarlyBird(
            string programName,
            byte[] payload,
            nuint length,
            SimulationStepService.StepCallbackDelegate? callback
        );
    }
}
