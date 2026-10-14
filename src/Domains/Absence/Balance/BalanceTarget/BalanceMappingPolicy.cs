namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance mapping policy.</summary>
public static class BalanceMappingPolicy
{
    /// <summary>Names the band a balance mapping score falls in.</summary>
    public static string ClassifyBalanceMapping(double score)
    {
        if (score < 6.4)
        {
            return "minimal";
        }
        if (score < 38.4)
        {
            return "low";
        }
        if (score < 58.3)
        {
            return "ok";
        }
        return "elevated";
    }

    public static double BalanceMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BalanceMappingSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 23494.0, in order.</summary>
    public static int BalanceMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23494.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
