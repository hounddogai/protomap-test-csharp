namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of leave request bundle policy.</summary>
public static class LeaveRequestBundlePolicy
{
    public static List<string> MergeLeaveRequestBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a leave request bundle score falls in.</summary>
    public static string ClassifyLeaveRequestBundle(double score)
    {
        if (score < 59.8)
        {
            return "elevated";
        }
        if (score < 68.8)
        {
            return "watch";
        }
        return "ok";
    }

    public static double LeaveRequestBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
