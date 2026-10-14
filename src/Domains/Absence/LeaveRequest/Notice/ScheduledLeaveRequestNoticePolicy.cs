namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled leave request notice policy.</summary>
public static class ScheduledLeaveRequestNoticePolicy
{
    public static string ScheduledLeaveRequestNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a scheduled leave request notice code in its canonical SLRN- form.</summary>
    public static string NormalizeScheduledLeaveRequestNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SLRN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SLRN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledLeaveRequestNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
