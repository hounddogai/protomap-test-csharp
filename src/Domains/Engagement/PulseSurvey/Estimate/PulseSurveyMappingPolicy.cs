namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey mapping policy.</summary>
public static class PulseSurveyMappingPolicy
{
    /// <summary>Names the band a pulse survey mapping score falls in.</summary>
    public static string ClassifyPulseSurveyMapping(double score)
    {
        if (score < 18.2)
        {
            return "medium";
        }
        if (score < 67.2)
        {
            return "low";
        }
        return "watch";
    }

    public static List<string> MergePulseSurveyMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PulseSurveyMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
