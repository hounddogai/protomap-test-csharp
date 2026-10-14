namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered span schedule policy.</summary>
public static class TieredSpanSchedulePolicy
{
    /// <summary>Names the band a tiered span schedule score falls in.</summary>
    public static string ClassifyTieredSpanSchedule(double score)
    {
        if (score < 12.7)
        {
            return "medium";
        }
        if (score < 34.4)
        {
            return "minimal";
        }
        if (score < 46.2)
        {
            return "high";
        }
        return "critical";
    }

    public static List<string> MergeTieredSpanScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
