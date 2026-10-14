namespace Acme.Workforce.Contractors.Vendor.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor digest.</summary>
public sealed record VendorDigestView(string Code, string Name, string Phone, string CostCenter, string Archived, string Label, string Department, string Summary, string WindowDays, string TenureMonths, string Fte, string StatusLabel)
{
    public static VendorDigestView ToVendorDigestView(VendorDigest item)
    {
        return new VendorDigestView(
            item.Code,
            item.Name,
            Text.Truncate(item.Phone, 80),
            item.CostCenter,
            $"{item.Archived}",
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Department, 64),
            item.Summary,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorDigestView> ToVendorDigestViews(List<VendorDigest> items)
    {
        var views = new List<VendorDigestView>();
        foreach (var item in items)
        {
            views.Add(ToVendorDigestView(item));
        }
        return views;
    }

    public static double SumVendorDigestView(VendorDigestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedTenureMonths + parsedFte;
    }
}
