using System.Security.Cryptography;
using System.Text;

namespace SciArticle.Models.Utilities;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        // Convert the string to bytes
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256.ComputeHash(bytes);
            
        // Convert the byte array to a hex string
        StringBuilder sb = new();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}