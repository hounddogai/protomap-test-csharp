namespace Acme.Workforce.Talent.Mobility.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling mobility proposal.</summary>
public sealed record RollingMobilityProposalView(string Code, string Label, string ExpiresAt, string NotesText, string Source, string StatusLabel)
{
    public static RollingMobilityProposalView ToRollingMobilityProposalView(RollingMobilityProposal item)
    {
        return new RollingMobilityProposalView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingMobilityProposalView> ToRollingMobilityProposalViews(List<RollingMobilityProposal> items)
    {
        var views = new List<RollingMobilityProposalView>();
        foreach (var item in items)
        {
            views.Add(ToRollingMobilityProposalView(item));
        }
        return views;
    }

    public static double SumRollingMobilityProposalView(RollingMobilityProposalView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
