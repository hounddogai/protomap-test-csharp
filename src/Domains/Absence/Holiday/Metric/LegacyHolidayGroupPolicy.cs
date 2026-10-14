namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy holiday group policy.</summary>
public static class LegacyHolidayGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1608.0, in order.</summary>
    public static int LegacyHolidayGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1608.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy holiday group score falls in.</summary>
    public static string ClassifyLegacyHolidayGroup(double score)
    {
        if (score < 2.6)
        {
            return "ok";
        }
        if (score < 9.8)
        {
            return "low";
        }
        if (score < 24.5)
        {
            return "high";
        }
        if (score < 62.9)
        {
            return "minimal";
        }
        return "medium";
    }

    public static double LegacyHolidayGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
