namespace Acme.Workforce.Assets.Warranty.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered warranty manifest.</summary>
public sealed record TieredWarrantyManifestView(string Code, string Name, string Region, string Country, string Notes, string ContactEmail, string Quantity, string TitleText, string StatusLabel)
{
    public static TieredWarrantyManifestView ToTieredWarrantyManifestView(TieredWarrantyManifest item)
    {
        return new TieredWarrantyManifestView(
            item.Code,
            item.Name,
            item.Region,
            Text.Truncate(item.Country, 80),
            item.Notes,
            Text.Truncate(item.ContactEmail, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredWarrantyManifestView> ToTieredWarrantyManifestViews(List<TieredWarrantyManifest> items)
    {
        var views = new List<TieredWarrantyManifestView>();
        foreach (var item in items)
        {
            views.Add(ToTieredWarrantyManifestView(item));
        }
        return views;
    }

    public static double SumTieredWarrantyManifestView(TieredWarrantyManifestView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
