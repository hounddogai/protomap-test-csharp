namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary position proposal.</summary>
public sealed record PrimaryPositionProposalView(string Code, string LabelText, string StartsAt, string EndsAtText, string Priority, string StatusLabel)
{
    public static PrimaryPositionProposalView ToPrimaryPositionProposalView(PrimaryPositionProposal item)
    {
        return new PrimaryPositionProposalView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPositionProposalView> ToPrimaryPositionProposalViews(List<PrimaryPositionProposal> items)
    {
        var views = new List<PrimaryPositionProposalView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPositionProposalView(item));
        }
        return views;
    }

    public static double SumPrimaryPositionProposalView(PrimaryPositionProposalView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedEndsAt + (double)parsedPriority;
    }
}
