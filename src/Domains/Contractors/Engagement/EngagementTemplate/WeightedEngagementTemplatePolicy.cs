namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted engagement template policy.</summary>
public static class WeightedEngagementTemplatePolicy
{
    public static string WeightedEngagementTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeWeightedEngagementTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a weighted engagement template score falls in.</summary>
    public static string ClassifyWeightedEngagementTemplate(double score)
    {
        if (score < 54.4)
        {
            return "elevated";
        }
        if (score < 54.5)
        {
            return "medium";
        }
        if (score < 88.4)
        {
            return "watch";
        }
        if (score < 99.4)
        {
            return "critical";
        }
        return "ok";
    }
}
