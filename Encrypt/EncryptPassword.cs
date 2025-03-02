using System.Security.Cryptography;

namespace TgPetProject.Encrypt;

using Aes = Aes;

public class EncryptPassword(byte[] key, byte[] iv)
{
    public async Task<string> Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        await using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        await using var writer = new StreamWriter(cs);
        await writer.WriteAsync(plainText);
        writer.Close();

        return Convert.ToBase64String(ms.ToArray());
    }

    public async Task<string> Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);

        return await reader.ReadToEndAsync();
    }
}