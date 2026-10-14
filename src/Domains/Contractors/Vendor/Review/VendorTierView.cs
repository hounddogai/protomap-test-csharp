namespace Acme.Workforce.Contractors.Vendor.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor tier.</summary>
public sealed record VendorTierView(string Code, string Name, string Cost, string Country, string Version, string FloorValueText, string WeightText, string ContactEmailText, string StatusLabel)
{
    public static VendorTierView ToVendorTierView(VendorTier item)
    {
        return new VendorTierView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorTierView> ToVendorTierViews(List<VendorTier> items)
    {
        var views = new List<VendorTierView>();
        foreach (var item in items)
        {
            views.Add(ToVendorTierView(item));
        }
        return views;
    }

    public static double SumVendorTierView(VendorTierView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedVersion + parsedFloorValue;
    }
}
