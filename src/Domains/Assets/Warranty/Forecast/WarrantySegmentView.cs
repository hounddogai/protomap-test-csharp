namespace Acme.Workforce.Assets.Warranty.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty segment.</summary>
public sealed record WarrantySegmentView(string Code, string Label, string NameText, string CapacityText, string StartsAt, string Channel, string StatusLabel)
{
    public static WarrantySegmentView ToWarrantySegmentView(WarrantySegment item)
    {
        return new WarrantySegmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.Name, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantySegmentView> ToWarrantySegmentViews(List<WarrantySegment> items)
    {
        var views = new List<WarrantySegmentView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantySegmentView(item));
        }
        return views;
    }

    public static double SumWarrantySegmentView(WarrantySegmentView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedStartsAt;
    }
}
