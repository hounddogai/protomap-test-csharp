namespace Acme.Workforce.Compliance.Certification.Slot;

using System.Globalization;

/// <summary>How screens and exports show a certification metric.</summary>
public sealed record CertificationMetricView(string Code, string Label, string GraceDays, string OwnerId, string Timezone, string TenureMonths, string ChecksumText, string CostText, string StatusLabel)
{
    public static CertificationMetricView ToCertificationMetricView(CertificationMetric item)
    {
        return new CertificationMetricView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Timezone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationMetricView> ToCertificationMetricViews(List<CertificationMetric> items)
    {
        var views = new List<CertificationMetricView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationMetricView(item));
        }
        return views;
    }

    public static double SumCertificationMetricView(CertificationMetricView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedTenureMonths + parsedCost;
    }
}
