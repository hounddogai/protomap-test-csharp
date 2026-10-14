namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a feedback policy.</summary>
public sealed record FeedbackPolicyView(string Code, string TitleText, string Ratio, string Locale, string Budget, string Margin, string StatusLabel)
{
    public static FeedbackPolicyView ToFeedbackPolicyView(FeedbackPolicy item)
    {
        return new FeedbackPolicyView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackPolicyView> ToFeedbackPolicyViews(List<FeedbackPolicy> items)
    {
        var views = new List<FeedbackPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackPolicyView(item));
        }
        return views;
    }

    public static double SumFeedbackPolicyView(FeedbackPolicyView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedRatio + parsedBudget + parsedMargin;
    }
}
