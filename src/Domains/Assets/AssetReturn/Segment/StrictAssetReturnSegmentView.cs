namespace Acme.Workforce.Assets.AssetReturn.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict asset return segment.</summary>
public sealed record StrictAssetReturnSegmentView(string Code, string Label, string Archived, string Source, string Retries, string Phone, string LeaveDays, string Badge, string Locale, string Quantity, string PriorityText, string StatusLabel)
{
    public static StrictAssetReturnSegmentView ToStrictAssetReturnSegmentView(StrictAssetReturnSegment item)
    {
        return new StrictAssetReturnSegmentView(
            item.Code,
            item.Label,
            $"{item.Archived}",
            item.Source,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            Text.Truncate(item.Locale, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAssetReturnSegmentView> ToStrictAssetReturnSegmentViews(List<StrictAssetReturnSegment> items)
    {
        var views = new List<StrictAssetReturnSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAssetReturnSegmentView(item));
        }
        return views;
    }

    public static double SumStrictAssetReturnSegmentView(StrictAssetReturnSegmentView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedLeaveDays + (double)parsedQuantity;
    }
}
