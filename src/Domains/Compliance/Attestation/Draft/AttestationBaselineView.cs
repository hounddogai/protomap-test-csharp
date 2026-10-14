namespace Acme.Workforce.Compliance.Attestation.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation baseline.</summary>
public sealed record AttestationBaselineView(string Code, string Title, string SequenceText, string OwnerIdText, string VersionText, string Name, string BadgeText, string TenureMonths, string Source, string BatchSizeText, string Reviewer, string StatusLabel)
{
    public static AttestationBaselineView ToAttestationBaselineView(AttestationBaseline item)
    {
        return new AttestationBaselineView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Name,
            Text.Truncate(item.Badge, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationBaselineView> ToAttestationBaselineViews(List<AttestationBaseline> items)
    {
        var views = new List<AttestationBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationBaselineView(item));
        }
        return views;
    }

    public static double SumAttestationBaselineView(AttestationBaselineView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedVersion + (double)parsedTenureMonths;
    }
}
