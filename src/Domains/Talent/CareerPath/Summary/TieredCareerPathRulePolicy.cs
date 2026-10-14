namespace Acme.Workforce.Talent.CareerPath.Summary;

/// <summary>Functions of tiered career path rule policy.</summary>
public static class TieredCareerPathRulePolicy
{
    /// <summary>Puts a tiered career path rule code in its canonical TCPR- form.</summary>
    public static string NormalizeTieredCareerPathRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TCPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TCPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredCareerPathRuleSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 27693.0, in order.</summary>
    public static int TieredCareerPathRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27693.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
