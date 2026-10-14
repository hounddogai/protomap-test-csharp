namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient occupancy plan.</summary>
public sealed record LenientOccupancyPlanView(string Code, string Title, string Weight, string Amount, string LocationText, string CostCenterText, string FloorValueText, string StatusLabel)
{
    public static LenientOccupancyPlanView ToLenientOccupancyPlanView(LenientOccupancyPlan item)
    {
        return new LenientOccupancyPlanView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            item.CostCenter,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientOccupancyPlanView> ToLenientOccupancyPlanViews(List<LenientOccupancyPlan> items)
    {
        var views = new List<LenientOccupancyPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLenientOccupancyPlanView(item));
        }
        return views;
    }

    public static double SumLenientOccupancyPlanView(LenientOccupancyPlanView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedAmount + parsedFloorValue;
    }
}
