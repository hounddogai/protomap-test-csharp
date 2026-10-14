namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using System.Globalization;

/// <summary>How screens and exports show a vendor mapping.</summary>
public sealed record VendorMappingView(string Code, string Title, string Fte, string VisibleText, string Skill, string StatusLabel)
{
    public static VendorMappingView ToVendorMappingView(VendorMapping item)
    {
        return new VendorMappingView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorMappingView> ToVendorMappingViews(List<VendorMapping> items)
    {
        var views = new List<VendorMappingView>();
        foreach (var item in items)
        {
            views.Add(ToVendorMappingView(item));
        }
        return views;
    }

    public static double SumVendorMappingView(VendorMappingView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
