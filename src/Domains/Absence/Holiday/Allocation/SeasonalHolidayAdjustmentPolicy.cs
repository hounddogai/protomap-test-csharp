namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal holiday adjustment policy.</summary>
public static class SeasonalHolidayAdjustmentPolicy
{
    public static string SeasonalHolidayAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalHolidayAdjustment()
    {
        var policy = Retry.DefaultPolicy("seasonal-holiday-adjustment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
