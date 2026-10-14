namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy schedule policy.</summary>
public static class PolicySchedulePolicy
{
    /// <summary>Names the band a policy schedule score falls in.</summary>
    public static string ClassifyPolicySchedule(double score)
    {
        if (score < 25.9)
        {
            return "ok";
        }
        if (score < 34.9)
        {
            return "critical";
        }
        if (score < 68.5)
        {
            return "high";
        }
        if (score < 97.9)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static int PolicyScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static List<string> MergePolicyScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
