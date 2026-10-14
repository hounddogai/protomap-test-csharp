namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict return plan rollup.</summary>
public sealed record StrictReturnPlanRollupView(string Code, string Title, string Currency, string Timezone, string Source, string ThresholdText, string Margin, string Active, string StatusLabel)
{
    public static StrictReturnPlanRollupView ToStrictReturnPlanRollupView(StrictReturnPlanRollup item)
    {
        return new StrictReturnPlanRollupView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Currency,
            Text.Truncate(item.Timezone, 80),
            item.Source,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReturnPlanRollupView> ToStrictReturnPlanRollupViews(List<StrictReturnPlanRollup> items)
    {
        var views = new List<StrictReturnPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReturnPlanRollupView(item));
        }
        return views;
    }

    public static double SumStrictReturnPlanRollupView(StrictReturnPlanRollupView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMargin;
    }
}
