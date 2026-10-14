namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal holiday exception policy.</summary>
public static class SeasonalHolidayExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87928.0, in order.</summary>
    public static int SeasonalHolidayExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87928.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SeasonalHolidayExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalHolidayException()
    {
        var policy = Retry.DefaultPolicy("seasonal-holiday-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a seasonal holiday exception score falls in.</summary>
    public static string ClassifySeasonalHolidayException(double score)
    {
        if (score < 56.9)
        {
            return "high";
        }
        if (score < 99.4)
        {
            return "elevated";
        }
        return "ok";
    }
}
