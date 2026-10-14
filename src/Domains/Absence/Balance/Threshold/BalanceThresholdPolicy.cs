namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance threshold policy.</summary>
public static class BalanceThresholdPolicy
{
    public static int BalanceThresholdSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a balance threshold score falls in.</summary>
    public static string ClassifyBalanceThreshold(double score)
    {
        if (score < 14.6)
        {
            return "critical";
        }
        if (score < 33.4)
        {
            return "elevated";
        }
        if (score < 49.0)
        {
            return "ok";
        }
        if (score < 81.6)
        {
            return "medium";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 36502.0, in order.</summary>
    public static int BalanceThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36502.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBalanceThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
