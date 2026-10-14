namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict scenario rule policy.</summary>
public static class StrictScenarioRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 84047.0, in order.</summary>
    public static int StrictScenarioRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84047.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a strict scenario rule code in its canonical SSR- form.</summary>
    public static string NormalizeStrictScenarioRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string StrictScenarioRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
