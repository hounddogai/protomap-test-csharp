namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy milestone policy.</summary>
public static class PolicyMilestonePolicy
{
    public static List<string> MergePolicyMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a policy milestone score falls in.</summary>
    public static string ClassifyPolicyMilestone(double score)
    {
        if (score < 19.0)
        {
            return "ok";
        }
        if (score < 39.0)
        {
            return "medium";
        }
        if (score < 91.2)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Puts a policy milestone code in its canonical PM- form.</summary>
    public static string NormalizePolicyMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PolicyMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
