namespace Acme.Workforce.Engagement.Feedback.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback review.</summary>
public sealed record FeedbackReviewView(string Code, string TitleText, string LocationText, string EmployeeId, string OwnerId, string Budget, string ExpiresAtText, string StatusLabel)
{
    public static FeedbackReviewView ToFeedbackReviewView(FeedbackReview item)
    {
        return new FeedbackReviewView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Location,
            item.EmployeeId,
            Text.Truncate(item.OwnerId, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackReviewView> ToFeedbackReviewViews(List<FeedbackReview> items)
    {
        var views = new List<FeedbackReviewView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackReviewView(item));
        }
        return views;
    }

    public static double SumFeedbackReviewView(FeedbackReviewView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedExpiresAt;
    }
}
