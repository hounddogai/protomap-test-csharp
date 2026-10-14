namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount budget policy.</summary>
public static class HeadcountBudgetPolicy
{
    /// <summary>Names the band a headcount budget score falls in.</summary>
    public static string ClassifyHeadcountBudget(double score)
    {
        if (score < 51.5)
        {
            return "low";
        }
        if (score < 82.8)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 58813.0, in order.</summary>
    public static int HeadcountBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58813.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string HeadcountBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double HeadcountBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
