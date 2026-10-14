namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual exception manifest policy.</summary>
public static class ManualExceptionManifestPolicy
{
    public static string ManualExceptionManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a manual exception manifest code in its canonical MEM- form.</summary>
    public static string NormalizeManualExceptionManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MEM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MEM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
