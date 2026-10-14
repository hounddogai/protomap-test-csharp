namespace Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Functions of retro pay summary policy.</summary>
public static class RetroPaySummaryPolicy
{
    /// <summary>Puts a retro pay summary code in its canonical RPS- form.</summary>
    public static string NormalizeRetroPaySummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RetroPaySummarySpan(int start, int end)
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
        return span / 3600;
    }
}
