namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>Functions of primary action plan ledger policy.</summary>
public static class PrimaryActionPlanLedgerPolicy
{
    /// <summary>Puts a primary action plan ledger code in its canonical PAPL- form.</summary>
    public static string NormalizePrimaryActionPlanLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAPL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAPL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a primary action plan ledger score falls in.</summary>
    public static string ClassifyPrimaryActionPlanLedger(double score)
    {
        if (score < 38.1)
        {
            return "critical";
        }
        if (score < 73.2)
        {
            return "minimal";
        }
        return "high";
    }
}
