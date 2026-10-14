namespace Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Functions of return plan ledger policy.</summary>
public static class ReturnPlanLedgerPolicy
{
    /// <summary>Puts a return plan ledger code in its canonical RPL- form.</summary>
    public static string NormalizeReturnPlanLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a return plan ledger score falls in.</summary>
    public static string ClassifyReturnPlanLedger(double score)
    {
        if (score < 45.2)
        {
            return "low";
        }
        if (score < 49.8)
        {
            return "minimal";
        }
        return "ok";
    }
}
