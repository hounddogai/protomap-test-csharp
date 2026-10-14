namespace Acme.Workforce.Compliance.Certification.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback certification bundle.</summary>
public sealed record FallbackCertificationBundleView(string Code, string NameText, string Grade, string TeamText, string TimezoneText, string QuantityText, string Revision, string StatusLabel)
{
    public static FallbackCertificationBundleView ToFallbackCertificationBundleView(FallbackCertificationBundle item)
    {
        return new FallbackCertificationBundleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Grade,
            item.Team,
            item.Timezone,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackCertificationBundleView> ToFallbackCertificationBundleViews(List<FallbackCertificationBundle> items)
    {
        var views = new List<FallbackCertificationBundleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackCertificationBundleView(item));
        }
        return views;
    }

    public static double SumFallbackCertificationBundleView(FallbackCertificationBundleView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedRevision;
    }
}
