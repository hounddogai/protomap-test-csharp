namespace Acme.Workforce.Compliance.Attestation.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation digest.</summary>
public sealed record AttestationDigestView(string Code, string Title, string TenureMonths, string Version, string StatusLabel)
{
    public static AttestationDigestView ToAttestationDigestView(AttestationDigest item)
    {
        return new AttestationDigestView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationDigestView> ToAttestationDigestViews(List<AttestationDigest> items)
    {
        var views = new List<AttestationDigestView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationDigestView(item));
        }
        return views;
    }

    public static double SumAttestationDigestView(AttestationDigestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedVersion;
    }
}
