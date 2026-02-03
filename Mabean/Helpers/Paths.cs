using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mabean.Helpers
{
    internal static class Paths
    {
        public static readonly string LocalData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string MabeanDir = Path.Combine(LocalData, "Mabean");
        public static readonly string DataDir = Path.Combine(MabeanDir, "data");
        public static readonly string KeyBinPath = Path.Combine(DataDir, "key.bin");
        public static readonly string PayloadsDir = Path.Combine(DataDir, "payloads");
        public static readonly string ConfigJsonPath = Path.Combine(PayloadsDir, "payloads.json");
        public static readonly string Dlls = Path.Combine(DataDir, "Dlls");
        public static readonly string Injection = Path.Combine(Dlls, "injection");
        public static readonly string Logs = Path.Combine(DataDir, "Logs");
    }
}
