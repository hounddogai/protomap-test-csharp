namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient retro pay summary policy.</summary>
public static class LenientRetroPaySummaryPolicy
{
    /// <summary>Puts a lenient retro pay summary code in its canonical LRPS- form.</summary>
    public static string NormalizeLenientRetroPaySummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LenientRetroPaySummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
