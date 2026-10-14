namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Functions of primary scenario tier policy.</summary>
public static class PrimaryScenarioTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31947.0, in order.</summary>
    public static int PrimaryScenarioTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31947.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary scenario tier code in its canonical PST- form.</summary>
    public static string NormalizePrimaryScenarioTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
