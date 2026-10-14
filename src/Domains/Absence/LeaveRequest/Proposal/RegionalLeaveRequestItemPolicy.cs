namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional leave request item policy.</summary>
public static class RegionalLeaveRequestItemPolicy
{
    public static List<string> MergeRegionalLeaveRequestItemLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a regional leave request item score falls in.</summary>
    public static string ClassifyRegionalLeaveRequestItem(double score)
    {
        if (score < 34.9)
        {
            return "critical";
        }
        if (score < 40.4)
        {
            return "low";
        }
        if (score < 75.4)
        {
            return "high";
        }
        return "elevated";
    }

    public static string RegionalLeaveRequestItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
