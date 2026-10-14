namespace Acme.Workforce.Contractors.Vendor.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor baseline.</summary>
public sealed record VendorBaselineView(string CodeText, string TitleText, string Fte, string EmployeeId, string Category, string StatusLabel)
{
    public static VendorBaselineView ToVendorBaselineView(VendorBaseline item)
    {
        return new VendorBaselineView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorBaselineView> ToVendorBaselineViews(List<VendorBaseline> items)
    {
        var views = new List<VendorBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToVendorBaselineView(item));
        }
        return views;
    }

    public static double SumVendorBaselineView(VendorBaselineView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
