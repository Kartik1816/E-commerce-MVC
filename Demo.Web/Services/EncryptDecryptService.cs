using System.Security.Cryptography;
using System.Text;

namespace Demo.Web.Services;

public class EncryptDecryptService
{
 public static string EncryptId(int id)
    {
        var key = "wMZ7f9k8h2L3p5Q9v8x2k7=="; 
        var plainText = id.ToString();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16]; 
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                var base64String = Convert.ToBase64String(encryptedBytes);
                return base64String.Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }
    }

    public static int DecryptId(string id)
    {
        var key = "wMZ7f9k8h2L3p5Q9v8x2k7=="; 
        id = id.Replace("-", "+").Replace("_", "/"); 
        switch (id.Length % 4) 
        {
            case 2: id += "=="; break;
            case 3: id += "="; break;
        }
        var encryptedBytes = Convert.FromBase64String(id);
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16]; 
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return int.Parse(Encoding.UTF8.GetString(decryptedBytes));
            }
        }
    }
}
