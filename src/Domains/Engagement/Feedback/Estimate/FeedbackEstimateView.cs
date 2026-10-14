namespace Acme.Workforce.Engagement.Feedback.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback estimate.</summary>
public sealed record FeedbackEstimateView(string Code, string Title, string Skill, string Currency, string ExpiresAtText, string Description, string Pinned, string StatusLabel)
{
    public static FeedbackEstimateView ToFeedbackEstimateView(FeedbackEstimate item)
    {
        return new FeedbackEstimateView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Skill, 80),
            item.Currency,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackEstimateView> ToFeedbackEstimateViews(List<FeedbackEstimate> items)
    {
        var views = new List<FeedbackEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackEstimateView(item));
        }
        return views;
    }

    public static double SumFeedbackEstimateView(FeedbackEstimateView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
