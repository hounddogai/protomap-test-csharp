namespace Acme.Workforce.Workplace.Desk.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk checkpoint.</summary>
public sealed record DeskCheckpointView(string CodeText, string Label, string Archived, string SegmentText, string MaximumText, string StatusLabel)
{
    public static DeskCheckpointView ToDeskCheckpointView(DeskCheckpoint item)
    {
        return new DeskCheckpointView(
            item.Code,
            item.Label,
            $"{item.Archived}",
            Text.Truncate(item.Segment, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskCheckpointView> ToDeskCheckpointViews(List<DeskCheckpoint> items)
    {
        var views = new List<DeskCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToDeskCheckpointView(item));
        }
        return views;
    }

    public static double SumDeskCheckpointView(DeskCheckpointView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
