namespace Acme.Workforce.Engagement.Feedback.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback target.</summary>
public sealed record FeedbackTargetView(string CodeText, string Title, string Summary, string EndsAt, string CreatedAtText, string Country, string ContactEmail, string StatusLabel)
{
    public static FeedbackTargetView ToFeedbackTargetView(FeedbackTarget item)
    {
        return new FeedbackTargetView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Summary, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackTargetView> ToFeedbackTargetViews(List<FeedbackTarget> items)
    {
        var views = new List<FeedbackTargetView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackTargetView(item));
        }
        return views;
    }

    public static double SumFeedbackTargetView(FeedbackTargetView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedCreatedAt;
    }
}
