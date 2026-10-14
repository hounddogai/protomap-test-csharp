namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan group.</summary>
public sealed record FloorPlanGroupView(string Code, string Label, string ReviewerText, string RateText, string Country, string Locale, string CostCenter, string StatusLabel)
{
    public static FloorPlanGroupView ToFloorPlanGroupView(FloorPlanGroup item)
    {
        return new FloorPlanGroupView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Reviewer,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            Text.Truncate(item.Locale, 64),
            Text.Truncate(item.CostCenter, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanGroupView> ToFloorPlanGroupViews(List<FloorPlanGroup> items)
    {
        var views = new List<FloorPlanGroupView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanGroupView(item));
        }
        return views;
    }

    public static double SumFloorPlanGroupView(FloorPlanGroupView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
