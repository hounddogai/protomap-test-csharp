namespace Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Functions of nomination threshold policy.</summary>
public static class NominationThresholdPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26015.0, in order.</summary>
    public static int NominationThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26015.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a nomination threshold score falls in.</summary>
    public static string ClassifyNominationThreshold(double score)
    {
        if (score < 3.0)
        {
            return "minimal";
        }
        if (score < 49.4)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Puts a nomination threshold code in its canonical NT- form.</summary>
    public static string NormalizeNominationThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
