namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of town hall snapshot policy.</summary>
public static class TownHallSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 85932.0, in order.</summary>
    public static int TownHallSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85932.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a town hall snapshot code in its canonical THS- form.</summary>
    public static string NormalizeTownHallSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a town hall snapshot score falls in.</summary>
    public static string ClassifyTownHallSnapshot(double score)
    {
        if (score < 8.8)
        {
            return "low";
        }
        if (score < 13.3)
        {
            return "minimal";
        }
        if (score < 91.9)
        {
            return "high";
        }
        return "ok";
    }

    public static double TownHallSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
