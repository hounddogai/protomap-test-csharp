namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan rollup.</summary>
public sealed record SuccessionPlanRollupView(string Code, string Label, string Team, string TimezoneText, string SalaryBandText, string Location, string Quantity, string StatusLabel)
{
    public static SuccessionPlanRollupView ToSuccessionPlanRollupView(SuccessionPlanRollup item)
    {
        return new SuccessionPlanRollupView(
            item.Code,
            item.Label,
            item.Team,
            item.Timezone,
            Text.Truncate(item.SalaryBand, 120),
            item.Location,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanRollupView> ToSuccessionPlanRollupViews(List<SuccessionPlanRollup> items)
    {
        var views = new List<SuccessionPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanRollupView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanRollupView(SuccessionPlanRollupView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
