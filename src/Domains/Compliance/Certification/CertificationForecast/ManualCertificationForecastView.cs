namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using System.Globalization;

/// <summary>How screens and exports show a manual certification forecast.</summary>
public sealed record ManualCertificationForecastView(string Code, string Name, string SalaryBand, string Version, string FteText, string ExternalRef, string EndsAtText, string Margin, string StatusLabel)
{
    public static ManualCertificationForecastView ToManualCertificationForecastView(ManualCertificationForecast item)
    {
        return new ManualCertificationForecastView(
            item.Code,
            item.Name,
            item.SalaryBand,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualCertificationForecastView> ToManualCertificationForecastViews(List<ManualCertificationForecast> items)
    {
        var views = new List<ManualCertificationForecastView>();
        foreach (var item in items)
        {
            views.Add(ToManualCertificationForecastView(item));
        }
        return views;
    }

    public static double SumManualCertificationForecastView(ManualCertificationForecastView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedFte + (double)parsedEndsAt;
    }
}
