namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling engagement digest policy.</summary>
public static class RollingEngagementDigestPolicy
{
    public static int RollingEngagementDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static double RollingEngagementDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 63553.0, in order.</summary>
    public static int RollingEngagementDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63553.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RollingEngagementDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
