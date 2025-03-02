using Newtonsoft.Json;
using System;
using System.IO;

namespace TgPetProject.Crypt
{
    public class CryptReader
    {
        public static (byte[] key, byte[] iv) ReadKeyAndIv(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var encryptionKeys = JsonConvert.DeserializeObject<EncryptionKeys>(json);
            return (encryptionKeys.Key, encryptionKeys.IV);
        }
    }

    public class EncryptionKeys
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}