namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled span tier policy.</summary>
public static class ScheduledSpanTierPolicy
{
    /// <summary>Names the band a scheduled span tier score falls in.</summary>
    public static string ClassifyScheduledSpanTier(double score)
    {
        if (score < 16.1)
        {
            return "ok";
        }
        if (score < 25.1)
        {
            return "watch";
        }
        if (score < 42.9)
        {
            return "high";
        }
        if (score < 91.1)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static List<string> MergeScheduledSpanTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
