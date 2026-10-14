namespace Acme.Workforce.Compliance.Certification.Budget;

using System.Globalization;

/// <summary>How screens and exports show a certification bundle.</summary>
public sealed record CertificationBundleView(string Code, string LabelText, string ExpiresAt, string BatchSize, string HoursText, string NameText, string WindowDaysText, string GraceDays, string RetriesText, string Threshold, string StatusLabel)
{
    public static CertificationBundleView ToCertificationBundleView(CertificationBundle item)
    {
        return new CertificationBundleView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationBundleView> ToCertificationBundleViews(List<CertificationBundle> items)
    {
        var views = new List<CertificationBundleView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationBundleView(item));
        }
        return views;
    }

    public static double SumCertificationBundleView(CertificationBundleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedBatchSize + parsedHours;
    }
}
