namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy exception profile policy.</summary>
public static class LegacyExceptionProfilePolicy
{
    /// <summary>Puts a legacy exception profile code in its canonical LEP- form.</summary>
    public static string NormalizeLegacyExceptionProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LEP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LEP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyExceptionProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
