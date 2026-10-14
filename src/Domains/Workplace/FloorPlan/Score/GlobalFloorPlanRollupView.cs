namespace Acme.Workforce.Workplace.FloorPlan.Score;

using System.Globalization;

/// <summary>How screens and exports show a global floor plan rollup.</summary>
public sealed record GlobalFloorPlanRollupView(string Code, string Label, string Reviewer, string MarginText, string UpdatedAt, string TenureMonthsText, string Phone, string Timezone, string StatusLabel)
{
    public static GlobalFloorPlanRollupView ToGlobalFloorPlanRollupView(GlobalFloorPlanRollup item)
    {
        return new GlobalFloorPlanRollupView(
            item.Code,
            item.Label,
            item.Reviewer,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalFloorPlanRollupView> ToGlobalFloorPlanRollupViews(List<GlobalFloorPlanRollup> items)
    {
        var views = new List<GlobalFloorPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalFloorPlanRollupView(item));
        }
        return views;
    }

    public static double SumGlobalFloorPlanRollupView(GlobalFloorPlanRollupView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedUpdatedAt + (double)parsedTenureMonths;
    }
}
