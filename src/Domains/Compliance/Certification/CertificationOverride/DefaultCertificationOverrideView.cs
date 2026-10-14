namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using System.Globalization;

/// <summary>How screens and exports show a default certification override.</summary>
public sealed record DefaultCertificationOverrideView(string CodeText, string TitleText, string Badge, string RatingText, string Approved, string Cost, string StatusLabel)
{
    public static DefaultCertificationOverrideView ToDefaultCertificationOverrideView(DefaultCertificationOverride item)
    {
        return new DefaultCertificationOverrideView(
            item.Code,
            item.Title,
            item.Badge,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultCertificationOverrideView> ToDefaultCertificationOverrideViews(List<DefaultCertificationOverride> items)
    {
        var views = new List<DefaultCertificationOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultCertificationOverrideView(item));
        }
        return views;
    }

    public static double SumDefaultCertificationOverrideView(DefaultCertificationOverrideView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedRating + parsedCost;
    }
}
