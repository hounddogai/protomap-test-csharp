namespace Acme.Workforce.Compliance.Attestation.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation manifest.</summary>
public sealed record AttestationManifestView(string Code, string LabelText, string TitleText, string HoursText, string Amount, string Version, string StatusLabel)
{
    public static AttestationManifestView ToAttestationManifestView(AttestationManifest item)
    {
        return new AttestationManifestView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationManifestView> ToAttestationManifestViews(List<AttestationManifest> items)
    {
        var views = new List<AttestationManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationManifestView(item));
        }
        return views;
    }

    public static double SumAttestationManifestView(AttestationManifestView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedHours + parsedAmount + (double)parsedVersion;
    }
}
