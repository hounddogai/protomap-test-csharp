namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy desk slot policy.</summary>
public static class LegacyDeskSlotPolicy
{
    public static List<string> MergeLegacyDeskSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy desk slot score falls in.</summary>
    public static string ClassifyLegacyDeskSlot(double score)
    {
        if (score < 56.0)
        {
            return "low";
        }
        if (score < 58.9)
        {
            return "medium";
        }
        return "watch";
    }
}
