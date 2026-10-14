namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal succession plan rule.</summary>
public sealed record SeasonalSuccessionPlanRuleView(string Code, string Title, string LimitText, string Quantity, string Pinned, string StatusLabel)
{
    public static SeasonalSuccessionPlanRuleView ToSeasonalSuccessionPlanRuleView(SeasonalSuccessionPlanRule item)
    {
        return new SeasonalSuccessionPlanRuleView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalSuccessionPlanRuleView> ToSeasonalSuccessionPlanRuleViews(List<SeasonalSuccessionPlanRule> items)
    {
        var views = new List<SeasonalSuccessionPlanRuleView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalSuccessionPlanRuleView(item));
        }
        return views;
    }

    public static double SumSeasonalSuccessionPlanRuleView(SeasonalSuccessionPlanRuleView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedQuantity;
    }
}
