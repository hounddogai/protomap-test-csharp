namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan schedule.</summary>
public sealed record FloorPlanScheduleView(string Code, string NameText, string WindowDays, string Visible, string FloorValueText, string ExpiresAtText, string Phone, string Retries, string FteText, string SalaryBandText, string Channel, string StatusLabel)
{
    public static FloorPlanScheduleView ToFloorPlanScheduleView(FloorPlanSchedule item)
    {
        return new FloorPlanScheduleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanScheduleView> ToFloorPlanScheduleViews(List<FloorPlanSchedule> items)
    {
        var views = new List<FloorPlanScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanScheduleView(item));
        }
        return views;
    }

    public static double SumFloorPlanScheduleView(FloorPlanScheduleView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue + (double)parsedExpiresAt;
    }
}
