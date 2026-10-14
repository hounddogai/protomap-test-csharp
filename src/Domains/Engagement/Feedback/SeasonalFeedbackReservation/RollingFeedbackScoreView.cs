namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling feedback score.</summary>
public sealed record RollingFeedbackScoreView(string Code, string Name, string Budget, string Channel, string Sequence, string DurationMinutes, string Rate, string AttemptsText, string GraceDaysText, string BatchSize, string StatusLabel)
{
    public static RollingFeedbackScoreView ToRollingFeedbackScoreView(RollingFeedbackScore item)
    {
        return new RollingFeedbackScoreView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingFeedbackScoreView> ToRollingFeedbackScoreViews(List<RollingFeedbackScore> items)
    {
        var views = new List<RollingFeedbackScoreView>();
        foreach (var item in items)
        {
            views.Add(ToRollingFeedbackScoreView(item));
        }
        return views;
    }

    public static double SumRollingFeedbackScoreView(RollingFeedbackScoreView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedSequence + (double)parsedDurationMinutes;
    }
}
