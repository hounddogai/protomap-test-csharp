namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

/// <summary>Functions of fallback exception tier policy.</summary>
public static class FallbackExceptionTierPolicy
{
    public static int FallbackExceptionTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a fallback exception tier code in its canonical FET- form.</summary>
    public static string NormalizeFallbackExceptionTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FET-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FET-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
