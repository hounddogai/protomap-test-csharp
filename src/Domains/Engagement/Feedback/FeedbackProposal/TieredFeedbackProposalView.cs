namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered feedback proposal.</summary>
public sealed record TieredFeedbackProposalView(string Code, string Name, string Segment, string OwnerId, string DescriptionText, string Hours, string Currency, string Margin, string StatusLabel)
{
    public static TieredFeedbackProposalView ToTieredFeedbackProposalView(TieredFeedbackProposal item)
    {
        return new TieredFeedbackProposalView(
            item.Code,
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Segment, 40),
            item.OwnerId,
            item.Description,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredFeedbackProposalView> ToTieredFeedbackProposalViews(List<TieredFeedbackProposal> items)
    {
        var views = new List<TieredFeedbackProposalView>();
        foreach (var item in items)
        {
            views.Add(ToTieredFeedbackProposalView(item));
        }
        return views;
    }

    public static double SumTieredFeedbackProposalView(TieredFeedbackProposalView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedHours + parsedMargin;
    }
}
