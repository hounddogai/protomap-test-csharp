namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy feedback proposal.</summary>
public sealed record LegacyFeedbackProposalView(string Code, string LabelText, string UpdatedAt, string Notes, string PriorityText, string Skill, string Capacity, string StatusLabel)
{
    public static LegacyFeedbackProposalView ToLegacyFeedbackProposalView(LegacyFeedbackProposal item)
    {
        return new LegacyFeedbackProposalView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyFeedbackProposalView> ToLegacyFeedbackProposalViews(List<LegacyFeedbackProposal> items)
    {
        var views = new List<LegacyFeedbackProposalView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyFeedbackProposalView(item));
        }
        return views;
    }

    public static double SumLegacyFeedbackProposalView(LegacyFeedbackProposalView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedPriority + (double)parsedCapacity;
    }
}
