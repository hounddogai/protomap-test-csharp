namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy pool segment.</summary>
public sealed record LegacyPoolSegmentView(string CodeText, string NameText, string Threshold, string ExternalRef, string Label, string Category, string StatusLabel)
{
    public static LegacyPoolSegmentView ToLegacyPoolSegmentView(LegacyPoolSegment item)
    {
        return new LegacyPoolSegmentView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPoolSegmentView> ToLegacyPoolSegmentViews(List<LegacyPoolSegment> items)
    {
        var views = new List<LegacyPoolSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPoolSegmentView(item));
        }
        return views;
    }

    public static double SumLegacyPoolSegmentView(LegacyPoolSegmentView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
