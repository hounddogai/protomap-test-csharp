namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled holiday snapshot policy.</summary>
public static class ScheduledHolidaySnapshotPolicy
{
    public static string ScheduledHolidaySnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledHolidaySnapshot()
    {
        var policy = Retry.DefaultPolicy("scheduled-holiday-snapshot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
