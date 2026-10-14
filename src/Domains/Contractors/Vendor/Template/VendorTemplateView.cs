namespace Acme.Workforce.Contractors.Vendor.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor template.</summary>
public sealed record VendorTemplateView(string Code, string NameText, string LocationText, string Version, string OwnerIdText, string DescriptionText, string Weight, string StatusLabel)
{
    public static VendorTemplateView ToVendorTemplateView(VendorTemplate item)
    {
        return new VendorTemplateView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Location,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            item.Description,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorTemplateView> ToVendorTemplateViews(List<VendorTemplate> items)
    {
        var views = new List<VendorTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToVendorTemplateView(item));
        }
        return views;
    }

    public static double SumVendorTemplateView(VendorTemplateView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedWeight;
    }
}
