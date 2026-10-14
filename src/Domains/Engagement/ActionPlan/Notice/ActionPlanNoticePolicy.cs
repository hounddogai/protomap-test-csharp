namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of action plan notice policy.</summary>
public static class ActionPlanNoticePolicy
{
    public static List<string> MergeActionPlanNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a action plan notice code in its canonical APN- form.</summary>
    public static string NormalizeActionPlanNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a action plan notice score falls in.</summary>
    public static string ClassifyActionPlanNotice(double score)
    {
        if (score < 16.6)
        {
            return "watch";
        }
        if (score < 46.8)
        {
            return "medium";
        }
        if (score < 53.5)
        {
            return "critical";
        }
        if (score < 58.3)
        {
            return "high";
        }
        return "ok";
    }

    public static double ActionPlanNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.09;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
