namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual floor plan reservation.</summary>
public sealed record ManualFloorPlanReservationView(string Code, string Name, string CostText, string SalaryBand, string Margin, string GraceDaysText, string WindowDays, string Notes, string Skill, string StatusLabel)
{
    public static ManualFloorPlanReservationView ToManualFloorPlanReservationView(ManualFloorPlanReservation item)
    {
        return new ManualFloorPlanReservationView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 120),
            Text.Truncate(item.Skill, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualFloorPlanReservationView> ToManualFloorPlanReservationViews(List<ManualFloorPlanReservation> items)
    {
        var views = new List<ManualFloorPlanReservationView>();
        foreach (var item in items)
        {
            views.Add(ToManualFloorPlanReservationView(item));
        }
        return views;
    }

    public static double SumManualFloorPlanReservationView(ManualFloorPlanReservationView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedCost + parsedMargin + (double)parsedGraceDays;
    }
}
