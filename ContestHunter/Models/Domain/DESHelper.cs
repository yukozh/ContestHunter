﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;

namespace ContestHunter.Models.Domain
{
    static class DESHelper
    {
        static byte[] IV;
        static byte[] Key;

        static DESHelper()
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                des.GenerateIV();
                des.GenerateKey();
                IV = new byte[] { 251, 186, 193, 202, 240, 1, 248, 44 };
                Key = new byte[] { 122, 96, 163, 60, 124, 253, 148, 105 };
            }
        }


        public static byte[] Encrypt(byte[] input)
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                using (ICryptoTransform encryptor = des.CreateEncryptor(Key, IV))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (CryptoStream stream = new CryptoStream(mem, encryptor, CryptoStreamMode.Write))
                        {
                            stream.Write(input, 0, input.Length);
                        }
                        return mem.ToArray();
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] input)
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                using (ICryptoTransform decryptor = des.CreateDecryptor(Key, IV))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (CryptoStream stream = new CryptoStream(mem, decryptor, CryptoStreamMode.Write))
                        {
                            stream.Write(input, 0, input.Length);
                        }
                        return mem.ToArray();
                    }
                }

            }
        }
    }
}