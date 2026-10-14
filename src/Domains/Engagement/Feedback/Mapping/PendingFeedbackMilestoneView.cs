namespace Acme.Workforce.Engagement.Feedback.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending feedback milestone.</summary>
public sealed record PendingFeedbackMilestoneView(string Code, string NameText, string GraceDaysText, string ScoreText, string EmployeeId, string Hours, string Attempts, string ArchivedText, string LocationText, string LeaveDaysText, string StatusLabel)
{
    public static PendingFeedbackMilestoneView ToPendingFeedbackMilestoneView(PendingFeedbackMilestone item)
    {
        return new PendingFeedbackMilestoneView(
            item.Code,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Location, 80),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingFeedbackMilestoneView> ToPendingFeedbackMilestoneViews(List<PendingFeedbackMilestone> items)
    {
        var views = new List<PendingFeedbackMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToPendingFeedbackMilestoneView(item));
        }
        return views;
    }

    public static double SumPendingFeedbackMilestoneView(PendingFeedbackMilestoneView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedScore + parsedHours;
    }
}
