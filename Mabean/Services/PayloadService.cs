using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mabean.Services
{
    public class PayloadService
    {
        private PayloadConfig? _config;
        public PayloadService()
        {
            Console.WriteLine("LoadPayloads called");
        }

        private async Task FetchJsonAsync()
        {
            if (File.Exists(Paths.ConfigJsonPath))
            {
                var json = await File.ReadAllTextAsync(Paths.ConfigJsonPath);
                _config = JsonSerializer.Deserialize<PayloadConfig>(json);
            }
        }

        private class PayloadConfig
        {
            public List<string> Payloads { get; set; } = new List<string>();
        }

        public async Task<string[]?> GetPayloads()
        {
            await FetchJsonAsync();
            if (_config.Payloads.Count == 0)
            {

                return null;
            }

            Console.WriteLine("Payloads found: " + _config.Payloads.Count);

            return _config.Payloads.ToArray();
        }

        public static async Task<byte[]> GetPayload(string payloadName)
        {
            var filePath = Path.Combine(Paths.PayloadsDir, payloadName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Payload not found", filePath);
            }
            var encodedPayload = await File.ReadAllTextAsync(filePath);
            var encryptedPayload = Convert.FromBase64String(encodedPayload);
            var decryptedPayload = await EncryptionService.XorDecrypt(encryptedPayload);
            return decryptedPayload;
        }

        public async Task<bool> AddPayload(string payload, string payloadName)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = payload,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };


            using (Process process = Process.Start(psi))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        process.StandardOutput.BaseStream.CopyTo(ms);
                        process.WaitForExit();
                        
                        var encryptedPayload = await EncryptionService.XorEncrypt(ms.ToArray());
                        string encoded = Convert.ToBase64String(encryptedPayload);

                        var json = File.ReadAllText(Paths.ConfigJsonPath);
                        await FetchJsonAsync();

                        _config.Payloads.Add(payloadName);

                        var node = JsonSerializer.SerializeToNode(_config, new JsonSerializerOptions { WriteIndented = true });

                        await File.WriteAllTextAsync(Paths.ConfigJsonPath, node.ToString());

                        File.WriteAllText(Path.Combine(Paths.PayloadsDir, payloadName), encoded);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
        }
    }
}
