namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling floor plan bundle.</summary>
public sealed record RollingFloorPlanBundleView(string Code, string LabelText, string StartsAt, string Reviewer, string Fte, string Badge, string Team, string EndsAt, string CapacityText, string Currency, string Priority, string StatusLabel)
{
    public static RollingFloorPlanBundleView ToRollingFloorPlanBundleView(RollingFloorPlanBundle item)
    {
        return new RollingFloorPlanBundleView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Team,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingFloorPlanBundleView> ToRollingFloorPlanBundleViews(List<RollingFloorPlanBundle> items)
    {
        var views = new List<RollingFloorPlanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToRollingFloorPlanBundleView(item));
        }
        return views;
    }

    public static double SumRollingFloorPlanBundleView(RollingFloorPlanBundleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedFte + (double)parsedEndsAt;
    }
}
