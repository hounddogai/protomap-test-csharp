namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive desk claim policy.</summary>
public static class AdaptiveDeskClaimPolicy
{
    /// <summary>Names the band a adaptive desk claim score falls in.</summary>
    public static string ClassifyAdaptiveDeskClaim(double score)
    {
        if (score < 63.6)
        {
            return "watch";
        }
        if (score < 92.1)
        {
            return "minimal";
        }
        if (score < 94.8)
        {
            return "elevated";
        }
        if (score < 98.2)
        {
            return "medium";
        }
        return "high";
    }

    public static string AdaptiveDeskClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double AdaptiveDeskClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 13361.0, in order.</summary>
    public static int AdaptiveDeskClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13361.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
