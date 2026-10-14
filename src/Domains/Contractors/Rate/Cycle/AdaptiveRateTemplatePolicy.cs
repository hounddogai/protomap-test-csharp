namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive rate template policy.</summary>
public static class AdaptiveRateTemplatePolicy
{
    public static string AdaptiveRateTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeAdaptiveRateTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive rate template score falls in.</summary>
    public static string ClassifyAdaptiveRateTemplate(double score)
    {
        if (score < 17.8)
        {
            return "elevated";
        }
        if (score < 39.3)
        {
            return "minimal";
        }
        return "ok";
    }
}
