namespace Acme.Workforce.Compliance.Certification.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification claim.</summary>
public sealed record CertificationClaimView(string Code, string Name, string Timezone, string Channel, string GradeText, string Retries, string FloorValue, string TenureMonthsText, string Team, string ContactEmail, string CostCenterText, string StatusLabel)
{
    public static CertificationClaimView ToCertificationClaimView(CertificationClaim item)
    {
        return new CertificationClaimView(
            item.Code,
            item.Name,
            Text.Truncate(item.Timezone, 80),
            item.Channel,
            Text.Truncate(item.Grade, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.ContactEmail,
            Text.Truncate(item.CostCenter, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationClaimView> ToCertificationClaimViews(List<CertificationClaim> items)
    {
        var views = new List<CertificationClaimView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationClaimView(item));
        }
        return views;
    }

    public static double SumCertificationClaimView(CertificationClaimView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedFloorValue + (double)parsedTenureMonths;
    }
}
