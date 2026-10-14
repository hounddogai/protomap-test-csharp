namespace Acme.Workforce.Workplace.Occupancy.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered occupancy record.</summary>
public sealed record TieredOccupancyRecordView(string CodeText, string LabelText, string Cost, string SegmentText, string Capacity, string GradeText, string Priority, string StatusLabel)
{
    public static TieredOccupancyRecordView ToTieredOccupancyRecordView(TieredOccupancyRecord item)
    {
        return new TieredOccupancyRecordView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredOccupancyRecordView> ToTieredOccupancyRecordViews(List<TieredOccupancyRecord> items)
    {
        var views = new List<TieredOccupancyRecordView>();
        foreach (var item in items)
        {
            views.Add(ToTieredOccupancyRecordView(item));
        }
        return views;
    }

    public static double SumTieredOccupancyRecordView(TieredOccupancyRecordView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedCapacity + (double)parsedPriority;
    }
}
