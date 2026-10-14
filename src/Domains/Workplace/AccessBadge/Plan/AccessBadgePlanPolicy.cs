namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge plan policy.</summary>
public static class AccessBadgePlanPolicy
{
    public static int AccessBadgePlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 71477.0, in order.</summary>
    public static int AccessBadgePlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71477.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AccessBadgePlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a access badge plan score falls in.</summary>
    public static string ClassifyAccessBadgePlan(double score)
    {
        if (score < 14.1)
        {
            return "minimal";
        }
        if (score < 27.0)
        {
            return "low";
        }
        if (score < 62.7)
        {
            return "watch";
        }
        if (score < 88.2)
        {
            return "high";
        }
        return "medium";
    }
}
