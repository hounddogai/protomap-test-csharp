namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Functions of manual scenario rule policy.</summary>
public static class ManualScenarioRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 63751.0, in order.</summary>
    public static int ManualScenarioRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63751.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int ManualScenarioRuleSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a manual scenario rule code in its canonical MSR- form.</summary>
    public static string NormalizeManualScenarioRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
