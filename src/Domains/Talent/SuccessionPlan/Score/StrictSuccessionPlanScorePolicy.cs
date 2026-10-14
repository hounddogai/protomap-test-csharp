namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict succession plan score policy.</summary>
public static class StrictSuccessionPlanScorePolicy
{
    public static List<string> MergeStrictSuccessionPlanScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict succession plan score score falls in.</summary>
    public static string ClassifyStrictSuccessionPlanScore(double score)
    {
        if (score < 23.3)
        {
            return "elevated";
        }
        if (score < 88.2)
        {
            return "critical";
        }
        if (score < 92.4)
        {
            return "ok";
        }
        return "low";
    }
}
