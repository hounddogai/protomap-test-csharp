namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band baseline policy.</summary>
public static class PayBandBaselinePolicy
{
    public static string PayBandBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a pay band baseline code in its canonical PBB- form.</summary>
    public static string NormalizePayBandBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
