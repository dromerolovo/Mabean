using Mabean.Helpers;
using Mabean.Services;
using System.IO;
using System.Runtime.InteropServices;

namespace Mabean.Interop
{
    internal static class InteropPrivilegeEscalation
    {
        public static void Init()
        {
            string dllPrivilegeEscalation = Path.Combine(Paths.Dlls, "2.dll");
            NativeLibrary.Load(dllPrivilegeEscalation);
        }

        [DllImport("2.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        public static extern int TokenTheftEscalation(
            nuint pid,
            SimulationStepService.StepCallbackDelegate? callback
        );

        [DllImport("2.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        public static extern int FodHelperAbuseEscalation(
            [MarshalAs(UnmanagedType.LPStr)] string? execPath,
            SimulationStepService.StepCallbackDelegate? callback
        );
    }
}
