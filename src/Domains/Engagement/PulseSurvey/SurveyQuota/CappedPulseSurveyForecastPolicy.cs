namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped pulse survey forecast policy.</summary>
public static class CappedPulseSurveyForecastPolicy
{
    /// <summary>Names the band a capped pulse survey forecast score falls in.</summary>
    public static string ClassifyCappedPulseSurveyForecast(double score)
    {
        if (score < 51.5)
        {
            return "watch";
        }
        if (score < 78.0)
        {
            return "medium";
        }
        return "high";
    }

    public static List<string> MergeCappedPulseSurveyForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CappedPulseSurveyForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
