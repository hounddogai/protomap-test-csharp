namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict floor plan bundle.</summary>
public sealed record StrictFloorPlanBundleView(string Code, string Name, string StartsAt, string Unit, string FloorValue, string SalaryBandText, string ApprovedText, string StatusLabel)
{
    public static StrictFloorPlanBundleView ToStrictFloorPlanBundleView(StrictFloorPlanBundle item)
    {
        return new StrictFloorPlanBundleView(
            item.Code,
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictFloorPlanBundleView> ToStrictFloorPlanBundleViews(List<StrictFloorPlanBundle> items)
    {
        var views = new List<StrictFloorPlanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictFloorPlanBundleView(item));
        }
        return views;
    }

    public static double SumStrictFloorPlanBundleView(StrictFloorPlanBundleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedFloorValue;
    }
}
