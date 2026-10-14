namespace Acme.Workforce.Assets.Warranty.Metric;

using System.Globalization;

/// <summary>How screens and exports show a warranty claim.</summary>
public sealed record WarrantyClaimView(string Code, string Name, string Attempts, string Rating, string Revision, string Timezone, string Fte, string ExternalRefText, string StatusLabel)
{
    public static WarrantyClaimView ToWarrantyClaimView(WarrantyClaim item)
    {
        return new WarrantyClaimView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyClaimView> ToWarrantyClaimViews(List<WarrantyClaim> items)
    {
        var views = new List<WarrantyClaimView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyClaimView(item));
        }
        return views;
    }

    public static double SumWarrantyClaimView(WarrantyClaimView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRating + (double)parsedRevision;
    }
}
