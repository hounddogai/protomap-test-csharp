namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual forecast policy.</summary>
public static class AccrualForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 7795.0, in order.</summary>
    public static int AccrualForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7795.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a accrual forecast score falls in.</summary>
    public static string ClassifyAccrualForecast(double score)
    {
        if (score < 8.9)
        {
            return "medium";
        }
        if (score < 65.4)
        {
            return "watch";
        }
        return "minimal";
    }

    public static string AccrualForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int AccrualForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
