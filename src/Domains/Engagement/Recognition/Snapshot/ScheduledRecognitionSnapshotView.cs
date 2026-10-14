namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled recognition snapshot.</summary>
public sealed record ScheduledRecognitionSnapshotView(string Code, string Title, string LeaveDaysText, string Priority, string CostCenter, string AttemptsText, string RegionText, string StatusLabel)
{
    public static ScheduledRecognitionSnapshotView ToScheduledRecognitionSnapshotView(ScheduledRecognitionSnapshot item)
    {
        return new ScheduledRecognitionSnapshotView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRecognitionSnapshotView> ToScheduledRecognitionSnapshotViews(List<ScheduledRecognitionSnapshot> items)
    {
        var views = new List<ScheduledRecognitionSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRecognitionSnapshotView(item));
        }
        return views;
    }

    public static double SumScheduledRecognitionSnapshotView(ScheduledRecognitionSnapshotView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedPriority + (double)parsedAttempts;
    }
}
