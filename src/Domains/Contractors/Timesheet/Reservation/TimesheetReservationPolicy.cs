namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of timesheet reservation policy.</summary>
public static class TimesheetReservationPolicy
{
    public static double TimesheetReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetReservation()
    {
        var policy = Retry.DefaultPolicy("timesheet-reservation");
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

    /// <summary>Puts a timesheet reservation code in its canonical TR- form.</summary>
    public static string NormalizeTimesheetReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
