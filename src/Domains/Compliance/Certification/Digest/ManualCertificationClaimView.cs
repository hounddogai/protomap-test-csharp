namespace Acme.Workforce.Compliance.Certification.Digest;

using System.Globalization;

/// <summary>How screens and exports show a manual certification claim.</summary>
public sealed record ManualCertificationClaimView(string Code, string Name, string EndsAt, string StartsAtText, string ExternalRefText, string StatusLabel)
{
    public static ManualCertificationClaimView ToManualCertificationClaimView(ManualCertificationClaim item)
    {
        return new ManualCertificationClaimView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualCertificationClaimView> ToManualCertificationClaimViews(List<ManualCertificationClaim> items)
    {
        var views = new List<ManualCertificationClaimView>();
        foreach (var item in items)
        {
            views.Add(ToManualCertificationClaimView(item));
        }
        return views;
    }

    public static double SumManualCertificationClaimView(ManualCertificationClaimView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedStartsAt;
    }
}
