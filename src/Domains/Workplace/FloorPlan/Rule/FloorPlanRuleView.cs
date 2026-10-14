namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan rule.</summary>
public sealed record FloorPlanRuleView(string CodeText, string Label, string JobCode, string Discount, string Region, string Source, string StatusLabel)
{
    public static FloorPlanRuleView ToFloorPlanRuleView(FloorPlanRule item)
    {
        return new FloorPlanRuleView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.JobCode,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            Text.Truncate(item.Source, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanRuleView> ToFloorPlanRuleViews(List<FloorPlanRule> items)
    {
        var views = new List<FloorPlanRuleView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanRuleView(item));
        }
        return views;
    }

    public static double SumFloorPlanRuleView(FloorPlanRuleView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
