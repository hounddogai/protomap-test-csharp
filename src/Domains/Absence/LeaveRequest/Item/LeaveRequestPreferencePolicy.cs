namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of leave request preference policy.</summary>
public static class LeaveRequestPreferencePolicy
{
    /// <summary>Names the band a leave request preference score falls in.</summary>
    public static string ClassifyLeaveRequestPreference(double score)
    {
        if (score < 32.6)
        {
            return "elevated";
        }
        if (score < 44.7)
        {
            return "ok";
        }
        if (score < 61.0)
        {
            return "minimal";
        }
        if (score < 73.5)
        {
            return "medium";
        }
        return "watch";
    }

    public static List<string> MergeLeaveRequestPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int LeaveRequestPreferenceSpan(int start, int end)
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
