namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of leave request rule policy.</summary>
public static class LeaveRequestRulePolicy
{
    /// <summary>Names the band a leave request rule score falls in.</summary>
    public static string ClassifyLeaveRequestRule(double score)
    {
        if (score < 11.9)
        {
            return "high";
        }
        if (score < 22.5)
        {
            return "ok";
        }
        if (score < 76.4)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergeLeaveRequestRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
