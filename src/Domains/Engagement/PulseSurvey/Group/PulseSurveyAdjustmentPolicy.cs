namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey adjustment policy.</summary>
public static class PulseSurveyAdjustmentPolicy
{
    public static int PulseSurveyAdjustmentSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergePulseSurveyAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a pulse survey adjustment score falls in.</summary>
    public static string ClassifyPulseSurveyAdjustment(double score)
    {
        if (score < 20.2)
        {
            return "critical";
        }
        if (score < 96.3)
        {
            return "ok";
        }
        return "elevated";
    }
}
