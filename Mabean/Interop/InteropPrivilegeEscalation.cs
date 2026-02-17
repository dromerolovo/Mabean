using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
            nuint pid
        );
    }
}
