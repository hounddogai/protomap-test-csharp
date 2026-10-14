namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of holiday assignment policy.</summary>
public static class HolidayAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayAssignment()
    {
        var policy = Retry.DefaultPolicy("holiday-assignment");
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

    public static int HolidayAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
