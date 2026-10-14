namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor window.</summary>
public sealed record VendorWindowView(string CodeText, string Label, string CountryText, string FloorValue, string Minimum, string Quantity, string SequenceText, string Name, string Limit, string StatusLabel)
{
    public static VendorWindowView ToVendorWindowView(VendorWindow item)
    {
        return new VendorWindowView(
            item.Code,
            item.Label,
            Text.Truncate(item.Country, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorWindowView> ToVendorWindowViews(List<VendorWindow> items)
    {
        var views = new List<VendorWindowView>();
        foreach (var item in items)
        {
            views.Add(ToVendorWindowView(item));
        }
        return views;
    }

    public static double SumVendorWindowView(VendorWindowView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMinimum + (double)parsedQuantity;
    }
}
