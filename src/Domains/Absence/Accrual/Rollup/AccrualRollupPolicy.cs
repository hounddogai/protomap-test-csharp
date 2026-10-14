namespace Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Functions of accrual rollup policy.</summary>
public static class AccrualRollupPolicy
{
    /// <summary>Puts a accrual rollup code in its canonical AR- form.</summary>
    public static string NormalizeAccrualRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a accrual rollup score falls in.</summary>
    public static string ClassifyAccrualRollup(double score)
    {
        if (score < 8.4)
        {
            return "low";
        }
        if (score < 25.0)
        {
            return "watch";
        }
        return "elevated";
    }
}
