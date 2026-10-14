namespace Acme.Workforce.Compliance.Certification.Draft;

using System.Globalization;

/// <summary>How screens and exports show a certification score.</summary>
public sealed record CertificationScoreView(string Code, string LabelText, string Channel, string Locale, string TeamText, string Retries, string CeilingText, string StatusLabel)
{
    public static CertificationScoreView ToCertificationScoreView(CertificationScore item)
    {
        return new CertificationScoreView(
            item.Code,
            item.Label,
            item.Channel,
            item.Locale,
            item.Team,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationScoreView> ToCertificationScoreViews(List<CertificationScore> items)
    {
        var views = new List<CertificationScoreView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationScoreView(item));
        }
        return views;
    }

    public static double SumCertificationScoreView(CertificationScoreView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCeiling;
    }
}
