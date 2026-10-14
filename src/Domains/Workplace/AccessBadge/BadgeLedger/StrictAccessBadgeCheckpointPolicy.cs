namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Functions of strict access badge checkpoint policy.</summary>
public static class StrictAccessBadgeCheckpointPolicy
{
    /// <summary>Names the band a strict access badge checkpoint score falls in.</summary>
    public static string ClassifyStrictAccessBadgeCheckpoint(double score)
    {
        if (score < 46.1)
        {
            return "watch";
        }
        if (score < 73.0)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Puts a strict access badge checkpoint code in its canonical SABC- form.</summary>
    public static string NormalizeStrictAccessBadgeCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SABC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SABC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 8528.0, in order.</summary>
    public static int StrictAccessBadgeCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8528.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
