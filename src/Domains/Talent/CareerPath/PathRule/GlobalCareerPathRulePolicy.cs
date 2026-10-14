namespace Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Functions of global career path rule policy.</summary>
public static class GlobalCareerPathRulePolicy
{
    /// <summary>Puts a global career path rule code in its canonical GCPR- form.</summary>
    public static string NormalizeGlobalCareerPathRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GCPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GCPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 64785.0, in order.</summary>
    public static int GlobalCareerPathRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64785.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
