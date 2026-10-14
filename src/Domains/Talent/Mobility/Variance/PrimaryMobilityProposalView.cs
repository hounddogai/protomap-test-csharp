namespace Acme.Workforce.Talent.Mobility.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary mobility proposal.</summary>
public sealed record PrimaryMobilityProposalView(string Code, string NameText, string Badge, string MinimumText, string Grade, string Archived, string Channel, string JobCodeText, string CreatedAt, string StatusLabel)
{
    public static PrimaryMobilityProposalView ToPrimaryMobilityProposalView(PrimaryMobilityProposal item)
    {
        return new PrimaryMobilityProposalView(
            item.Code,
            item.Name,
            Text.Truncate(item.Badge, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 120),
            $"{item.Archived}",
            item.Channel,
            item.JobCode,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryMobilityProposalView> ToPrimaryMobilityProposalViews(List<PrimaryMobilityProposal> items)
    {
        var views = new List<PrimaryMobilityProposalView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryMobilityProposalView(item));
        }
        return views;
    }

    public static double SumPrimaryMobilityProposalView(PrimaryMobilityProposalView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCreatedAt;
    }
}
