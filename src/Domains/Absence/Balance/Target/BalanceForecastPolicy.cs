namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance forecast policy.</summary>
public static class BalanceForecastPolicy
{
    /// <summary>Names the band a balance forecast score falls in.</summary>
    public static string ClassifyBalanceForecast(double score)
    {
        if (score < 13.4)
        {
            return "watch";
        }
        if (score < 50.6)
        {
            return "critical";
        }
        if (score < 64.4)
        {
            return "high";
        }
        return "low";
    }

    public static int BalanceForecastSpan(int start, int end)
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
        return span / 86400;
    }

    public static string BalanceForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 58782.0, in order.</summary>
    public static int BalanceForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58782.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
