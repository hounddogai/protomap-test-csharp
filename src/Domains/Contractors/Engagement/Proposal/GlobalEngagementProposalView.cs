namespace Acme.Workforce.Contractors.Engagement.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global engagement proposal.</summary>
public sealed record GlobalEngagementProposalView(string Code, string Name, string Quantity, string Rating, string ExternalRefText, string Location, string Archived, string StatusLabel)
{
    public static GlobalEngagementProposalView ToGlobalEngagementProposalView(GlobalEngagementProposal item)
    {
        return new GlobalEngagementProposalView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.Location, 40),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalEngagementProposalView> ToGlobalEngagementProposalViews(List<GlobalEngagementProposal> items)
    {
        var views = new List<GlobalEngagementProposalView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalEngagementProposalView(item));
        }
        return views;
    }

    public static double SumGlobalEngagementProposalView(GlobalEngagementProposalView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedRating;
    }
}
