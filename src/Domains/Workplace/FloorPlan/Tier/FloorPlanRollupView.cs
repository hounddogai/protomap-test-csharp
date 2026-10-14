namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan rollup.</summary>
public sealed record FloorPlanRollupView(string CodeText, string Name, string JobCode, string SalaryBand, string FteText, string Visible, string StatusLabel)
{
    public static FloorPlanRollupView ToFloorPlanRollupView(FloorPlanRollup item)
    {
        return new FloorPlanRollupView(
            item.Code,
            item.Name,
            item.JobCode,
            Text.Truncate(item.SalaryBand, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanRollupView> ToFloorPlanRollupViews(List<FloorPlanRollup> items)
    {
        var views = new List<FloorPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanRollupView(item));
        }
        return views;
    }

    public static double SumFloorPlanRollupView(FloorPlanRollupView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
