namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a seasonal feedback milestone.</summary>
public sealed record SeasonalFeedbackMilestoneView(string Code, string Name, string SummaryText, string ManagerIdText, string LeaveDaysText, string ExpiresAt, string UpdatedAt, string StatusLabel)
{
    public static SeasonalFeedbackMilestoneView ToSeasonalFeedbackMilestoneView(SeasonalFeedbackMilestone item)
    {
        return new SeasonalFeedbackMilestoneView(
            item.Code,
            item.Name,
            item.Summary,
            item.ManagerId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalFeedbackMilestoneView> ToSeasonalFeedbackMilestoneViews(List<SeasonalFeedbackMilestone> items)
    {
        var views = new List<SeasonalFeedbackMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalFeedbackMilestoneView(item));
        }
        return views;
    }

    public static double SumSeasonalFeedbackMilestoneView(SeasonalFeedbackMilestoneView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedExpiresAt + (double)parsedUpdatedAt;
    }
}
