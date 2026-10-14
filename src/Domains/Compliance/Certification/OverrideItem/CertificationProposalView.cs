namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification proposal.</summary>
public sealed record CertificationProposalView(string Code, string Name, string Quantity, string ContactEmailText, string Location, string Checksum, string BadgeText, string DurationMinutes, string StatusLabel)
{
    public static CertificationProposalView ToCertificationProposalView(CertificationProposal item)
    {
        return new CertificationProposalView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Location,
            item.Checksum,
            item.Badge,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationProposalView> ToCertificationProposalViews(List<CertificationProposal> items)
    {
        var views = new List<CertificationProposalView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationProposalView(item));
        }
        return views;
    }

    public static double SumCertificationProposalView(CertificationProposalView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedDurationMinutes;
    }
}
