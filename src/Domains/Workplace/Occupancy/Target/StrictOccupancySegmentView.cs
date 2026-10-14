namespace Acme.Workforce.Workplace.Occupancy.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict occupancy segment.</summary>
public sealed record StrictOccupancySegmentView(string Code, string Label, string AmountText, string ManagerId, string Region, string StatusLabel)
{
    public static StrictOccupancySegmentView ToStrictOccupancySegmentView(StrictOccupancySegment item)
    {
        return new StrictOccupancySegmentView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictOccupancySegmentView> ToStrictOccupancySegmentViews(List<StrictOccupancySegment> items)
    {
        var views = new List<StrictOccupancySegmentView>();
        foreach (var item in items)
        {
            views.Add(ToStrictOccupancySegmentView(item));
        }
        return views;
    }

    public static double SumStrictOccupancySegmentView(StrictOccupancySegmentView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
