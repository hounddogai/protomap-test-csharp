namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification override.</summary>
public sealed record CertificationOverrideView(string Code, string Label, string UpdatedAt, string Checksum, string OwnerId, string Sequence, string Pinned, string Source, string StatusLabel)
{
    public static CertificationOverrideView ToCertificationOverrideView(CertificationOverride item)
    {
        return new CertificationOverrideView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.OwnerId,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationOverrideView> ToCertificationOverrideViews(List<CertificationOverride> items)
    {
        var views = new List<CertificationOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationOverrideView(item));
        }
        return views;
    }

    public static double SumCertificationOverrideView(CertificationOverrideView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedSequence;
    }
}
