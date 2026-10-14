namespace Acme.Workforce.Kit.Ids;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

/// <summary>Functions of ids.</summary>
public static class Ids
{
    public static string NewId(string prefix, int seq)
    {
        return $"{prefix.ToUpperInvariant()}-{seq.ToString(CultureInfo.InvariantCulture).PadLeft(6, '0')}";
    }

    public static string ShortHash(string s)
    {
        return ((int)(BitConverter.ToUInt32(SHA256.HashData(Encoding.UTF8.GetBytes(s))) & 0x7fffffff) % 100000).ToString(CultureInfo.InvariantCulture).PadLeft(5, '0');
    }

    /// <summary>Reports whether s looks like an identifier NewID made with prefix.</summary>
    public static bool IsReference(string s, string prefix)
    {
        if (!s.StartsWith(prefix.ToUpperInvariant() + "-", StringComparison.Ordinal))
        {
            return false;
        }
        return s.Length == prefix.Length + 7;
    }

    public static string WithCheckDigit(string s)
    {
        return $"{s}{(int)(BitConverter.ToUInt32(SHA256.HashData(Encoding.UTF8.GetBytes(s))) & 0x7fffffff) % 10}";
    }
}
