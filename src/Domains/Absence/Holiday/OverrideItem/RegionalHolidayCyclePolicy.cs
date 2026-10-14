namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional holiday cycle policy.</summary>
public static class RegionalHolidayCyclePolicy
{
    /// <summary>Names the band a regional holiday cycle score falls in.</summary>
    public static string ClassifyRegionalHolidayCycle(double score)
    {
        if (score < 26.8)
        {
            return "low";
        }
        if (score < 94.5)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalHolidayCycle()
    {
        var policy = Retry.DefaultPolicy("regional-holiday-cycle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string RegionalHolidayCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
