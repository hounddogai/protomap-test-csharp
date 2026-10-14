namespace Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Functions of primary accrual plan policy.</summary>
public static class PrimaryAccrualPlanPolicy
{
    /// <summary>Names the band a primary accrual plan score falls in.</summary>
    public static string ClassifyPrimaryAccrualPlan(double score)
    {
        if (score < 21.4)
        {
            return "elevated";
        }
        if (score < 35.9)
        {
            return "ok";
        }
        if (score < 41.2)
        {
            return "watch";
        }
        if (score < 65.8)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Puts a primary accrual plan code in its canonical PAP- form.</summary>
    public static string NormalizePrimaryAccrualPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
