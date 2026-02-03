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
            string dllPath = Path.Combine(Paths.Injection, "SimpleDll.dll");
            NativeLibrary.Load(dllPath);
        }

        [DllImport("SimpleDll.dll",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
                public static extern int InjectPayload(
            uint pid,
            byte[] payload,
            uint length
        );
    }
}
