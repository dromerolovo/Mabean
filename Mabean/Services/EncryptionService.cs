using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.Services
{
    internal class EncryptionService
    {
        internal static async Task<byte[]> XorEncrypt(byte[] data)
        {
            var key = await File.ReadAllBytesAsync(Paths.KeyBinPath);


            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        internal static async Task<byte[]> XorDecrypt(byte[] data) => await XorEncrypt(data);
    }
}
