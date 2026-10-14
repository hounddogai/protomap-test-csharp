namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default visitor rule policy.</summary>
public static class DefaultVisitorRulePolicy
{
    public static List<string> MergeDefaultVisitorRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a default visitor rule score falls in.</summary>
    public static string ClassifyDefaultVisitorRule(double score)
    {
        if (score < 80.0)
        {
            return "critical";
        }
        if (score < 91.8)
        {
            return "watch";
        }
        if (score < 95.7)
        {
            return "elevated";
        }
        if (score < 97.7)
        {
            return "low";
        }
        return "medium";
    }

    public static int DefaultVisitorRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
