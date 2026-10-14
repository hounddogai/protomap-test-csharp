namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using System.Globalization;

/// <summary>How screens and exports show a scheduled reorg schedule.</summary>
public sealed record ScheduledReorgScheduleView(string Code, string Label, string Unit, string ReviewerText, string SequenceText, string StatusLabel)
{
    public static ScheduledReorgScheduleView ToScheduledReorgScheduleView(ScheduledReorgSchedule item)
    {
        return new ScheduledReorgScheduleView(
            item.Code,
            item.Label,
            item.Unit,
            item.Reviewer,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledReorgScheduleView> ToScheduledReorgScheduleViews(List<ScheduledReorgSchedule> items)
    {
        var views = new List<ScheduledReorgScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledReorgScheduleView(item));
        }
        return views;
    }

    public static double SumScheduledReorgScheduleView(ScheduledReorgScheduleView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
