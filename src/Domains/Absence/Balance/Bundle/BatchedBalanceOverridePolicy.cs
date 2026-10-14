namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched balance override policy.</summary>
public static class BatchedBalanceOverridePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 18744.0, in order.</summary>
    public static int BatchedBalanceOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18744.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched balance override score falls in.</summary>
    public static string ClassifyBatchedBalanceOverride(double score)
    {
        if (score < 5.4)
        {
            return "medium";
        }
        if (score < 11.6)
        {
            return "watch";
        }
        if (score < 50.3)
        {
            return "ok";
        }
        if (score < 68.1)
        {
            return "high";
        }
        return "low";
    }

    public static List<string> MergeBatchedBalanceOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int BatchedBalanceOverrideSpan(int start, int end)
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
}
