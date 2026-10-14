namespace Acme.Workforce.Contractors.Vendor.Request;

using System.Globalization;

/// <summary>How screens and exports show a lenient vendor batch.</summary>
public sealed record LenientVendorBatchView(string Code, string Label, string FloorValue, string Phone, string Summary, string Location, string Title, string StatusLabel)
{
    public static LenientVendorBatchView ToLenientVendorBatchView(LenientVendorBatch item)
    {
        return new LenientVendorBatchView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Summary,
            item.Location,
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientVendorBatchView> ToLenientVendorBatchViews(List<LenientVendorBatch> items)
    {
        var views = new List<LenientVendorBatchView>();
        foreach (var item in items)
        {
            views.Add(ToLenientVendorBatchView(item));
        }
        return views;
    }

    public static double SumLenientVendorBatchView(LenientVendorBatchView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFloorValue;
    }
}
