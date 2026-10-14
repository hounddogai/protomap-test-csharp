namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy floor plan rule.</summary>
public sealed record LegacyFloorPlanRuleView(string Code, string LabelText, string TenureMonthsText, string ReviewerText, string Weight, string Active, string WindowDays, string DiscountText, string SkillText, string StatusLabel)
{
    public static LegacyFloorPlanRuleView ToLegacyFloorPlanRuleView(LegacyFloorPlanRule item)
    {
        return new LegacyFloorPlanRuleView(
            item.Code,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyFloorPlanRuleView> ToLegacyFloorPlanRuleViews(List<LegacyFloorPlanRule> items)
    {
        var views = new List<LegacyFloorPlanRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyFloorPlanRuleView(item));
        }
        return views;
    }

    public static double SumLegacyFloorPlanRuleView(LegacyFloorPlanRuleView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedWeight + (double)parsedWindowDays;
    }
}
