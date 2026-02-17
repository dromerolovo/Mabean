using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Mabean.Interop
{
    internal static class InteropInit
    {
        public static void Init()
        {
            InteropInjection.Init();
            InteropPrivilegeEscalation.Init();
        }
    }
}
