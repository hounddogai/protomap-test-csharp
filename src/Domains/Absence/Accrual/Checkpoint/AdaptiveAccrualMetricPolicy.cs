namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive accrual metric policy.</summary>
public static class AdaptiveAccrualMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 77687.0, in order.</summary>
    public static int AdaptiveAccrualMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77687.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAdaptiveAccrualMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive accrual metric score falls in.</summary>
    public static string ClassifyAdaptiveAccrualMetric(double score)
    {
        if (score < 34.6)
        {
            return "critical";
        }
        if (score < 43.2)
        {
            return "ok";
        }
        if (score < 82.0)
        {
            return "medium";
        }
        return "watch";
    }

    public static string AdaptiveAccrualMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
