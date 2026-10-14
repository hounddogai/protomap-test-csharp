namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of town hall claim policy.</summary>
public static class TownHallClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 56858.0, in order.</summary>
    public static int TownHallClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56858.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TownHallClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a town hall claim score falls in.</summary>
    public static string ClassifyTownHallClaim(double score)
    {
        if (score < 23.7)
        {
            return "elevated";
        }
        if (score < 80.5)
        {
            return "watch";
        }
        return "minimal";
    }

    public static string TownHallClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
