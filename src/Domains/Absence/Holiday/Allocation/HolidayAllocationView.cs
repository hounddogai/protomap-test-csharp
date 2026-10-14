namespace Acme.Workforce.Absence.Holiday.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday allocation.</summary>
public sealed record HolidayAllocationView(string Code, string Name, string Sequence, string GraceDays, string FloorValue, string Margin, string Score, string Currency, string Retries, string StatusLabel)
{
    public static HolidayAllocationView ToHolidayAllocationView(HolidayAllocation item)
    {
        return new HolidayAllocationView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayAllocationView> ToHolidayAllocationViews(List<HolidayAllocation> items)
    {
        var views = new List<HolidayAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayAllocationView(item));
        }
        return views;
    }

    public static double SumHolidayAllocationView(HolidayAllocationView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedGraceDays + parsedFloorValue;
    }
}
