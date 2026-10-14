namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy accrual claim policy.</summary>
public static class LegacyAccrualClaimPolicy
{
    public static int LegacyAccrualClaimSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a legacy accrual claim score falls in.</summary>
    public static string ClassifyLegacyAccrualClaim(double score)
    {
        if (score < 50.8)
        {
            return "low";
        }
        if (score < 65.7)
        {
            return "minimal";
        }
        if (score < 85.9)
        {
            return "ok";
        }
        if (score < 86.3)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Puts a legacy accrual claim code in its canonical LAC- form.</summary>
    public static string NormalizeLegacyAccrualClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyAccrualClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
