namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional holiday window.</summary>
public sealed record RegionalHolidayWindowView(string Code, string LabelText, string Team, string OwnerId, string WeightText, string CostText, string FloorValue, string StatusLabel)
{
    public static RegionalHolidayWindowView ToRegionalHolidayWindowView(RegionalHolidayWindow item)
    {
        return new RegionalHolidayWindowView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Team,
            item.OwnerId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalHolidayWindowView> ToRegionalHolidayWindowViews(List<RegionalHolidayWindow> items)
    {
        var views = new List<RegionalHolidayWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalHolidayWindowView(item));
        }
        return views;
    }

    public static double SumRegionalHolidayWindowView(RegionalHolidayWindowView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCost + parsedFloorValue;
    }
}
