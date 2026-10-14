namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted headcount schedule policy.</summary>
public static class WeightedHeadcountSchedulePolicy
{
    /// <summary>Names the band a weighted headcount schedule score falls in.</summary>
    public static string ClassifyWeightedHeadcountSchedule(double score)
    {
        if (score < 28.5)
        {
            return "watch";
        }
        if (score < 49.3)
        {
            return "high";
        }
        if (score < 93.1)
        {
            return "critical";
        }
        if (score < 95.5)
        {
            return "elevated";
        }
        return "medium";
    }

    public static string WeightedHeadcountScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 98064.0, in order.</summary>
    public static int WeightedHeadcountScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98064.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
