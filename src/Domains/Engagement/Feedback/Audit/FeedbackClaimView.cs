namespace Acme.Workforce.Engagement.Feedback.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback claim.</summary>
public sealed record FeedbackClaimView(string Code, string Name, string LeaveDaysText, string RatioText, string Region, string Timezone, string StatusLabel)
{
    public static FeedbackClaimView ToFeedbackClaimView(FeedbackClaim item)
    {
        return new FeedbackClaimView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackClaimView> ToFeedbackClaimViews(List<FeedbackClaim> items)
    {
        var views = new List<FeedbackClaimView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackClaimView(item));
        }
        return views;
    }

    public static double SumFeedbackClaimView(FeedbackClaimView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRatio;
    }
}
