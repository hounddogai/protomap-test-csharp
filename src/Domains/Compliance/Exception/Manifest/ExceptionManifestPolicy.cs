namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception manifest policy.</summary>
public static class ExceptionManifestPolicy
{
    /// <summary>Puts a exception manifest code in its canonical EM- form.</summary>
    public static string NormalizeExceptionManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ExceptionManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
