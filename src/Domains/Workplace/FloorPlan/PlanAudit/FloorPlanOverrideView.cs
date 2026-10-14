namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan override.</summary>
public sealed record FloorPlanOverrideView(string Code, string LabelText, string Version, string MinimumText, string Retries, string Channel, string StatusLabel)
{
    public static FloorPlanOverrideView ToFloorPlanOverrideView(FloorPlanOverride item)
    {
        return new FloorPlanOverrideView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanOverrideView> ToFloorPlanOverrideViews(List<FloorPlanOverride> items)
    {
        var views = new List<FloorPlanOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanOverrideView(item));
        }
        return views;
    }

    public static double SumFloorPlanOverrideView(FloorPlanOverrideView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMinimum + (double)parsedRetries;
    }
}
