namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Functions of lenient garnishment mapping policy.</summary>
public static class LenientGarnishmentMappingPolicy
{
    public static int LenientGarnishmentMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a lenient garnishment mapping code in its canonical LGM- form.</summary>
    public static string NormalizeLenientGarnishmentMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LGM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LGM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
