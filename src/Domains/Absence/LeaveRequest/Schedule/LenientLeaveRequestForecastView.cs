namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient leave request forecast.</summary>
public sealed record LenientLeaveRequestForecastView(string Code, string Label, string Location, string Category, string MinimumText, string FloorValue, string StatusLabel)
{
    public static LenientLeaveRequestForecastView ToLenientLeaveRequestForecastView(LenientLeaveRequestForecast item)
    {
        return new LenientLeaveRequestForecastView(
            item.Code,
            item.Label,
            Text.Truncate(item.Location, 64),
            item.Category,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLeaveRequestForecastView> ToLenientLeaveRequestForecastViews(List<LenientLeaveRequestForecast> items)
    {
        var views = new List<LenientLeaveRequestForecastView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLeaveRequestForecastView(item));
        }
        return views;
    }

    public static double SumLenientLeaveRequestForecastView(LenientLeaveRequestForecastView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedFloorValue;
    }
}
