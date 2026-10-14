namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount variance policy.</summary>
public static class HeadcountVariancePolicy
{
    public static double HeadcountVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 73596.0, in order.</summary>
    public static int HeadcountVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73596.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a headcount variance score falls in.</summary>
    public static string ClassifyHeadcountVariance(double score)
    {
        if (score < 21.3)
        {
            return "ok";
        }
        if (score < 45.5)
        {
            return "critical";
        }
        if (score < 84.4)
        {
            return "elevated";
        }
        return "high";
    }
}
