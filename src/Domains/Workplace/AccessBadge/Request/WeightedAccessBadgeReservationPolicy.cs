namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted access badge reservation policy.</summary>
public static class WeightedAccessBadgeReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 78792.0, in order.</summary>
    public static int WeightedAccessBadgeReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78792.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string WeightedAccessBadgeReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a weighted access badge reservation score falls in.</summary>
    public static string ClassifyWeightedAccessBadgeReservation(double score)
    {
        if (score < 17.9)
        {
            return "medium";
        }
        if (score < 42.5)
        {
            return "minimal";
        }
        if (score < 57.3)
        {
            return "elevated";
        }
        if (score < 61.2)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAccessBadgeReservation()
    {
        var policy = Retry.DefaultPolicy("weighted-access-badge-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
