namespace Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Functions of manual span batch policy.</summary>
public static class ManualSpanBatchPolicy
{
    public static int ManualSpanBatchSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a manual span batch code in its canonical MSB- form.</summary>
    public static string NormalizeManualSpanBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MSB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MSB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
