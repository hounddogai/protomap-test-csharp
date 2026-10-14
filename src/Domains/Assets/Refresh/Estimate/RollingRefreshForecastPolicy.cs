namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling refresh forecast policy.</summary>
public static class RollingRefreshForecastPolicy
{
    /// <summary>Names the band a rolling refresh forecast score falls in.</summary>
    public static string ClassifyRollingRefreshForecast(double score)
    {
        if (score < 43.5)
        {
            return "low";
        }
        if (score < 71.2)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 31514.0, in order.</summary>
    public static int RollingRefreshForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31514.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RollingRefreshForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
