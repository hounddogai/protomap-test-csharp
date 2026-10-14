namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant bundle policy.</summary>
public static class EquityGrantBundlePolicy
{
    public static string EquityGrantBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a equity grant bundle score falls in.</summary>
    public static string ClassifyEquityGrantBundle(double score)
    {
        if (score < 13.3)
        {
            return "high";
        }
        if (score < 27.5)
        {
            return "low";
        }
        if (score < 49.9)
        {
            return "medium";
        }
        if (score < 63.3)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 92212.0, in order.</summary>
    public static int EquityGrantBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92212.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double EquityGrantBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
