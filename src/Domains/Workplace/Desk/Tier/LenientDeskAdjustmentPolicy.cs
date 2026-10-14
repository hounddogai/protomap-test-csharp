namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient desk adjustment policy.</summary>
public static class LenientDeskAdjustmentPolicy
{
    /// <summary>Names the band a lenient desk adjustment score falls in.</summary>
    public static string ClassifyLenientDeskAdjustment(double score)
    {
        if (score < 30.5)
        {
            return "high";
        }
        if (score < 49.5)
        {
            return "watch";
        }
        return "ok";
    }

    public static List<string> MergeLenientDeskAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
