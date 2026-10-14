namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan digest policy.</summary>
public static class SuccessionPlanDigestPolicy
{
    /// <summary>Names the band a succession plan digest score falls in.</summary>
    public static string ClassifySuccessionPlanDigest(double score)
    {
        if (score < 33.2)
        {
            return "elevated";
        }
        if (score < 40.4)
        {
            return "high";
        }
        if (score < 74.2)
        {
            return "watch";
        }
        if (score < 91.5)
        {
            return "critical";
        }
        return "minimal";
    }

    public static int SuccessionPlanDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 65034.0, in order.</summary>
    public static int SuccessionPlanDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65034.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SuccessionPlanDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
