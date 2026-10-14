namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of timesheet policy policy.</summary>
public static class TimesheetPolicyPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetPolicy()
    {
        var policy = Retry.DefaultPolicy("timesheet-policy");
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

    /// <summary>Puts a timesheet policy code in its canonical TP- form.</summary>
    public static string NormalizeTimesheetPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TimesheetPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
