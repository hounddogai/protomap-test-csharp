namespace Acme.Workforce.Compensation.Adjustment.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment segment.</summary>
public sealed record AdjustmentSegmentView(string CodeText, string Label, string Capacity, string CategoryText, string ArchivedText, string StartsAt, string StatusLabel)
{
    public static AdjustmentSegmentView ToAdjustmentSegmentView(AdjustmentSegment item)
    {
        return new AdjustmentSegmentView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            $"{item.Archived}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentSegmentView> ToAdjustmentSegmentViews(List<AdjustmentSegment> items)
    {
        var views = new List<AdjustmentSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentSegmentView(item));
        }
        return views;
    }

    public static double SumAdjustmentSegmentView(AdjustmentSegmentView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedStartsAt;
    }
}
