namespace Acme.Workforce.Assets.Assignment.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled assignment checkpoint.</summary>
public sealed record ScheduledAssignmentCheckpointView(string Code, string TitleText, string SourceText, string ManagerId, string SequenceText, string StatusLabel)
{
    public static ScheduledAssignmentCheckpointView ToScheduledAssignmentCheckpointView(ScheduledAssignmentCheckpoint item)
    {
        return new ScheduledAssignmentCheckpointView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Source,
            item.ManagerId,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssignmentCheckpointView> ToScheduledAssignmentCheckpointViews(List<ScheduledAssignmentCheckpoint> items)
    {
        var views = new List<ScheduledAssignmentCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssignmentCheckpointView(item));
        }
        return views;
    }

    public static double SumScheduledAssignmentCheckpointView(ScheduledAssignmentCheckpointView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
