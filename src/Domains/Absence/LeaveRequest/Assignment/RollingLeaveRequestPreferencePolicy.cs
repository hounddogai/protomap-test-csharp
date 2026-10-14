namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling leave request preference policy.</summary>
public static class RollingLeaveRequestPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingLeaveRequestPreference()
    {
        var policy = Retry.DefaultPolicy("rolling-leave-request-preference");
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

    public static List<string> MergeRollingLeaveRequestPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string RollingLeaveRequestPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double RollingLeaveRequestPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
