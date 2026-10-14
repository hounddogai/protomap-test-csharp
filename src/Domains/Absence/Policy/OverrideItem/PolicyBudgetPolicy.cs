namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy budget policy.</summary>
public static class PolicyBudgetPolicy
{
    /// <summary>Names the band a policy budget score falls in.</summary>
    public static string ClassifyPolicyBudget(double score)
    {
        if (score < 18.9)
        {
            return "high";
        }
        if (score < 19.8)
        {
            return "low";
        }
        if (score < 71.0)
        {
            return "ok";
        }
        if (score < 95.4)
        {
            return "medium";
        }
        return "minimal";
    }

    public static List<string> MergePolicyBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
