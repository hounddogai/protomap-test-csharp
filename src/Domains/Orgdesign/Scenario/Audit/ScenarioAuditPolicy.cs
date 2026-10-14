namespace Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Functions of scenario audit policy.</summary>
public static class ScenarioAuditPolicy
{
    public static int ScenarioAuditSpan(int start, int end)
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

    /// <summary>Puts a scenario audit code in its canonical SA- form.</summary>
    public static string NormalizeScenarioAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 54695.0, in order.</summary>
    public static int ScenarioAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54695.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
