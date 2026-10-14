namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global pool snapshot policy.</summary>
public static class GlobalPoolSnapshotPolicy
{
    public static string GlobalPoolSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a global pool snapshot code in its canonical GPS- form.</summary>
    public static string NormalizeGlobalPoolSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 99316.0, in order.</summary>
    public static int GlobalPoolSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99316.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a global pool snapshot score falls in.</summary>
    public static string ClassifyGlobalPoolSnapshot(double score)
    {
        if (score < 67.0)
        {
            return "high";
        }
        if (score < 93.1)
        {
            return "elevated";
        }
        return "ok";
    }
}
