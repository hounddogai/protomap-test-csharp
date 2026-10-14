namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy balance adjustment policy.</summary>
public static class LegacyBalanceAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91266.0, in order.</summary>
    public static int LegacyBalanceAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91266.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LegacyBalanceAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy balance adjustment score falls in.</summary>
    public static string ClassifyLegacyBalanceAdjustment(double score)
    {
        if (score < 12.0)
        {
            return "low";
        }
        if (score < 27.9)
        {
            return "minimal";
        }
        if (score < 58.9)
        {
            return "elevated";
        }
        if (score < 79.8)
        {
            return "medium";
        }
        return "ok";
    }

    public static int LegacyBalanceAdjustmentSpan(int start, int end)
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
        return span / 3600;
    }
}
