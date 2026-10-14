namespace Acme.Workforce.Compliance.Certification.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification digest.</summary>
public sealed record CertificationDigestView(string Code, string LabelText, string StartsAtText, string Phone, string Active, string Ceiling, string ScoreText, string CostText, string StatusLabel)
{
    public static CertificationDigestView ToCertificationDigestView(CertificationDigest item)
    {
        return new CertificationDigestView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            $"{item.Active}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationDigestView> ToCertificationDigestViews(List<CertificationDigest> items)
    {
        var views = new List<CertificationDigestView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationDigestView(item));
        }
        return views;
    }

    public static double SumCertificationDigestView(CertificationDigestView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedCeiling + parsedScore;
    }
}
