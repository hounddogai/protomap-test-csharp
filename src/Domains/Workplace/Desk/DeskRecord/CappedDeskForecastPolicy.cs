namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped desk forecast policy.</summary>
public static class CappedDeskForecastPolicy
{
    /// <summary>Names the band a capped desk forecast score falls in.</summary>
    public static string ClassifyCappedDeskForecast(double score)
    {
        if (score < 55.6)
        {
            return "critical";
        }
        if (score < 62.5)
        {
            return "medium";
        }
        if (score < 66.9)
        {
            return "low";
        }
        if (score < 89.7)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static List<string> MergeCappedDeskForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
