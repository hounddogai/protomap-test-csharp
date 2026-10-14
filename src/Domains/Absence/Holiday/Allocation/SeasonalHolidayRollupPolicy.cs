namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal holiday rollup policy.</summary>
public static class SeasonalHolidayRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 52379.0, in order.</summary>
    public static int SeasonalHolidayRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52379.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SeasonalHolidayRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double SeasonalHolidayRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal holiday rollup score falls in.</summary>
    public static string ClassifySeasonalHolidayRollup(double score)
    {
        if (score < 4.0)
        {
            return "watch";
        }
        if (score < 19.6)
        {
            return "high";
        }
        if (score < 71.9)
        {
            return "elevated";
        }
        if (score < 81.2)
        {
            return "medium";
        }
        return "ok";
    }
}
