namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Functions of succession plan entry policy.</summary>
public static class SuccessionPlanEntryPolicy
{
    /// <summary>Puts a succession plan entry code in its canonical SPE- form.</summary>
    public static string NormalizeSuccessionPlanEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 78380.0, in order.</summary>
    public static int SuccessionPlanEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78380.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a succession plan entry score falls in.</summary>
    public static string ClassifySuccessionPlanEntry(double score)
    {
        if (score < 18.4)
        {
            return "medium";
        }
        if (score < 77.6)
        {
            return "high";
        }
        return "minimal";
    }
}
