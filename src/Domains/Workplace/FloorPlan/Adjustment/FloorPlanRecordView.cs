namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan record.</summary>
public sealed record FloorPlanRecordView(string Code, string Name, string SequenceText, string ContactEmail, string BatchSizeText, string Source, string LimitText, string EndsAt, string Ceiling, string DurationMinutesText, string StatusLabel)
{
    public static FloorPlanRecordView ToFloorPlanRecordView(FloorPlanRecord item)
    {
        return new FloorPlanRecordView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanRecordView> ToFloorPlanRecordViews(List<FloorPlanRecord> items)
    {
        var views = new List<FloorPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanRecordView(item));
        }
        return views;
    }

    public static double SumFloorPlanRecordView(FloorPlanRecordView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedBatchSize + (double)parsedLimit;
    }
}
