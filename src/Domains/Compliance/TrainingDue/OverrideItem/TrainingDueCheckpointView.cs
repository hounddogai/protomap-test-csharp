namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a training due checkpoint.</summary>
public sealed record TrainingDueCheckpointView(string CodeText, string Title, string Sequence, string SegmentText, string Priority, string Limit, string Reviewer, string ExpiresAtText, string StatusLabel)
{
    public static TrainingDueCheckpointView ToTrainingDueCheckpointView(TrainingDueCheckpoint item)
    {
        return new TrainingDueCheckpointView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueCheckpointView> ToTrainingDueCheckpointViews(List<TrainingDueCheckpoint> items)
    {
        var views = new List<TrainingDueCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueCheckpointView(item));
        }
        return views;
    }

    public static double SumTrainingDueCheckpointView(TrainingDueCheckpointView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedPriority + (double)parsedLimit;
    }
}
