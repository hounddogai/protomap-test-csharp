namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal assignment allocation policy.</summary>
public static class SeasonalAssignmentAllocationPolicy
{
    /// <summary>Puts a seasonal assignment allocation code in its canonical SAA- form.</summary>
    public static string NormalizeSeasonalAssignmentAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal assignment allocation score falls in.</summary>
    public static string ClassifySeasonalAssignmentAllocation(double score)
    {
        if (score < 3.8)
        {
            return "minimal";
        }
        if (score < 6.6)
        {
            return "elevated";
        }
        if (score < 53.3)
        {
            return "critical";
        }
        if (score < 56.6)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 2873.0, in order.</summary>
    public static int SeasonalAssignmentAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2873.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SeasonalAssignmentAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
