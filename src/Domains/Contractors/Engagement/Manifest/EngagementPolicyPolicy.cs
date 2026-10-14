namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement policy policy.</summary>
public static class EngagementPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 39778.0, in order.</summary>
    public static int EngagementPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39778.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeEngagementPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double EngagementPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a engagement policy score falls in.</summary>
    public static string ClassifyEngagementPolicy(double score)
    {
        if (score < 8.4)
        {
            return "minimal";
        }
        if (score < 66.8)
        {
            return "low";
        }
        if (score < 81.8)
        {
            return "ok";
        }
        return "medium";
    }
}
