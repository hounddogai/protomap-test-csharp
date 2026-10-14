namespace Acme.Workforce.Compliance.Certification.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification estimate.</summary>
public sealed record CertificationEstimateView(string Code, string Title, string ExpiresAt, string Department, string Name, string ActiveText, string LabelText, string StatusLabel)
{
    public static CertificationEstimateView ToCertificationEstimateView(CertificationEstimate item)
    {
        return new CertificationEstimateView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Name,
            $"{item.Active}",
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationEstimateView> ToCertificationEstimateViews(List<CertificationEstimate> items)
    {
        var views = new List<CertificationEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationEstimateView(item));
        }
        return views;
    }

    public static double SumCertificationEstimateView(CertificationEstimateView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
