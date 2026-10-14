namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered offboarding segment policy.</summary>
public static class TieredOffboardingSegmentPolicy
{
    /// <summary>Names the band a tiered offboarding segment score falls in.</summary>
    public static string ClassifyTieredOffboardingSegment(double score)
    {
        if (score < 34.8)
        {
            return "watch";
        }
        if (score < 75.9)
        {
            return "ok";
        }
        if (score < 82.8)
        {
            return "elevated";
        }
        if (score < 98.3)
        {
            return "low";
        }
        return "critical";
    }

    public static int TieredOffboardingSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static string TieredOffboardingSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 57550.0, in order.</summary>
    public static int TieredOffboardingSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57550.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
