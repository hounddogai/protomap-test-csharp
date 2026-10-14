namespace Acme.Workforce.Contractors.Vendor.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a default vendor segment.</summary>
public sealed record DefaultVendorSegmentView(string Code, string Label, string Phone, string Approved, string Limit, string CostText, string Score, string Segment, string FloorValue, string Hours, string StatusLabel)
{
    public static DefaultVendorSegmentView ToDefaultVendorSegmentView(DefaultVendorSegment item)
    {
        return new DefaultVendorSegmentView(
            item.Code,
            item.Label,
            item.Phone,
            $"{item.Approved}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultVendorSegmentView> ToDefaultVendorSegmentViews(List<DefaultVendorSegment> items)
    {
        var views = new List<DefaultVendorSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultVendorSegmentView(item));
        }
        return views;
    }

    public static double SumDefaultVendorSegmentView(DefaultVendorSegmentView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedCost + parsedScore;
    }
}
