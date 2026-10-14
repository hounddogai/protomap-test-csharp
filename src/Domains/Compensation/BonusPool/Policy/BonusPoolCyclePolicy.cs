namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of bonus pool cycle policy.</summary>
public static class BonusPoolCyclePolicy
{
    /// <summary>Names the band a bonus pool cycle score falls in.</summary>
    public static string ClassifyBonusPoolCycle(double score)
    {
        if (score < 6.6)
        {
            return "critical";
        }
        if (score < 75.3)
        {
            return "elevated";
        }
        if (score < 79.0)
        {
            return "low";
        }
        return "medium";
    }

    public static double BonusPoolCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 64891.0, in order.</summary>
    public static int BonusPoolCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64891.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BonusPoolCycleSpan(int start, int end)
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
        return span / 86400;
    }
}
