namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of town hall digest policy.</summary>
public static class TownHallDigestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTownHallDigest()
    {
        var policy = Retry.DefaultPolicy("town-hall-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 60375.0, in order.</summary>
    public static int TownHallDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60375.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string TownHallDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a town hall digest score falls in.</summary>
    public static string ClassifyTownHallDigest(double score)
    {
        if (score < 33.3)
        {
            return "low";
        }
        if (score < 80.6)
        {
            return "high";
        }
        return "watch";
    }
}
