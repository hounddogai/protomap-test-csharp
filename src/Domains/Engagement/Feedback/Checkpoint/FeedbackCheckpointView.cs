namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback checkpoint.</summary>
public sealed record FeedbackCheckpointView(string CodeText, string NameText, string Locale, string Sequence, string ManagerId, string OwnerIdText, string DurationMinutes, string CurrencyText, string Grade, string Summary, string Version, string StatusLabel)
{
    public static FeedbackCheckpointView ToFeedbackCheckpointView(FeedbackCheckpoint item)
    {
        return new FeedbackCheckpointView(
            item.Code,
            item.Name,
            item.Locale,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.OwnerId,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.Grade, 40),
            item.Summary,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackCheckpointView> ToFeedbackCheckpointViews(List<FeedbackCheckpoint> items)
    {
        var views = new List<FeedbackCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackCheckpointView(item));
        }
        return views;
    }

    public static double SumFeedbackCheckpointView(FeedbackCheckpointView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedDurationMinutes + (double)parsedVersion;
    }
}
