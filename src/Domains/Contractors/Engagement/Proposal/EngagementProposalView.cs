namespace Acme.Workforce.Contractors.Engagement.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement proposal.</summary>
public sealed record EngagementProposalView(string Code, string NameText, string Archived, string DescriptionText, string ManagerId, string Visible, string AmountText, string StatusLabel)
{
    public static EngagementProposalView ToEngagementProposalView(EngagementProposal item)
    {
        return new EngagementProposalView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            $"{item.Archived}",
            item.Description,
            item.ManagerId,
            $"{item.Visible}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementProposalView> ToEngagementProposalViews(List<EngagementProposal> items)
    {
        var views = new List<EngagementProposalView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementProposalView(item));
        }
        return views;
    }

    public static double SumEngagementProposalView(EngagementProposalView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
