namespace Acme.Workforce.Orgdesign.Position.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position segment.</summary>
public sealed record PositionSegmentView(string Code, string LabelText, string ExpiresAt, string RatioText, string Source, string Checksum, string EndsAtText, string CategoryText, string CostCenter, string StatusLabel)
{
    public static PositionSegmentView ToPositionSegmentView(PositionSegment item)
    {
        return new PositionSegmentView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 120),
            item.Checksum,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            Text.Truncate(item.CostCenter, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionSegmentView> ToPositionSegmentViews(List<PositionSegment> items)
    {
        var views = new List<PositionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPositionSegmentView(item));
        }
        return views;
    }

    public static double SumPositionSegmentView(PositionSegmentView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedRatio + (double)parsedEndsAt;
    }
}
