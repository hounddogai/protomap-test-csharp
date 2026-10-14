namespace Acme.Workforce.Compliance.Attestation.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation rollup.</summary>
public sealed record AttestationRollupView(string Code, string Name, string ContactEmail, string ActiveText, string Checksum, string EmployeeIdText, string Maximum, string StatusLabel)
{
    public static AttestationRollupView ToAttestationRollupView(AttestationRollup item)
    {
        return new AttestationRollupView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.ContactEmail,
            $"{item.Active}",
            item.Checksum,
            item.EmployeeId,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationRollupView> ToAttestationRollupViews(List<AttestationRollup> items)
    {
        var views = new List<AttestationRollupView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationRollupView(item));
        }
        return views;
    }

    public static double SumAttestationRollupView(AttestationRollupView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
