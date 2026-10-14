namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan threshold.</summary>
public sealed record FloorPlanThresholdView(string CodeText, string TitleText, string GraceDays, string LocationText, string EndsAt, string Minimum, string Ratio, string StartsAt, string Archived, string SalaryBandText, string StatusLabel)
{
    public static FloorPlanThresholdView ToFloorPlanThresholdView(FloorPlanThreshold item)
    {
        return new FloorPlanThresholdView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.SalaryBand, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanThresholdView> ToFloorPlanThresholdViews(List<FloorPlanThreshold> items)
    {
        var views = new List<FloorPlanThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanThresholdView(item));
        }
        return views;
    }

    public static double SumFloorPlanThresholdView(FloorPlanThresholdView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedEndsAt + parsedMinimum;
    }
}
