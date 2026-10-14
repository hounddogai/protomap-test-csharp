namespace Acme.Workforce.Assets.Warranty.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict warranty manifest.</summary>
public sealed record StrictWarrantyManifestView(string Code, string Title, string Rating, string DescriptionText, string Location, string Phone, string ContactEmail, string UpdatedAt, string Channel, string Checksum, string StatusLabel)
{
    public static StrictWarrantyManifestView ToStrictWarrantyManifestView(StrictWarrantyManifest item)
    {
        return new StrictWarrantyManifestView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Location,
            Text.Truncate(item.Phone, 64),
            item.ContactEmail,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Checksum, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictWarrantyManifestView> ToStrictWarrantyManifestViews(List<StrictWarrantyManifest> items)
    {
        var views = new List<StrictWarrantyManifestView>();
        foreach (var item in items)
        {
            views.Add(ToStrictWarrantyManifestView(item));
        }
        return views;
    }

    public static double SumStrictWarrantyManifestView(StrictWarrantyManifestView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedUpdatedAt;
    }
}
