namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount metric policy.</summary>
public static class HeadcountMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 77445.0, in order.</summary>
    public static int HeadcountMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77445.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a headcount metric score falls in.</summary>
    public static string ClassifyHeadcountMetric(double score)
    {
        if (score < 5.0)
        {
            return "ok";
        }
        if (score < 11.7)
        {
            return "high";
        }
        if (score < 29.5)
        {
            return "elevated";
        }
        if (score < 55.0)
        {
            return "low";
        }
        return "watch";
    }

    public static string HeadcountMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
