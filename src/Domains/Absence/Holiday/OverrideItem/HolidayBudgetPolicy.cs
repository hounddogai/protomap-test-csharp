namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of holiday budget policy.</summary>
public static class HolidayBudgetPolicy
{
    public static string HolidayBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayBudget()
    {
        var policy = Retry.DefaultPolicy("holiday-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a holiday budget score falls in.</summary>
    public static string ClassifyHolidayBudget(double score)
    {
        if (score < 52.3)
        {
            return "critical";
        }
        if (score < 71.3)
        {
            return "medium";
        }
        if (score < 81.8)
        {
            return "minimal";
        }
        return "low";
    }
}
