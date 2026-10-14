namespace Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Functions of lenient desk allocation policy.</summary>
public static class LenientDeskAllocationPolicy
{
    /// <summary>Puts a lenient desk allocation code in its canonical LDA- form.</summary>
    public static string NormalizeLenientDeskAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LDA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LDA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientDeskAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
