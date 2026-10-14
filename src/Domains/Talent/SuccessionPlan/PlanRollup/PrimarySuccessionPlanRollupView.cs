namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using System.Globalization;

/// <summary>How screens and exports show a primary succession plan rollup.</summary>
public sealed record PrimarySuccessionPlanRollupView(string Code, string LabelText, string ChannelText, string Active, string LimitText, string Minimum, string Quantity, string Version, string StatusLabel)
{
    public static PrimarySuccessionPlanRollupView ToPrimarySuccessionPlanRollupView(PrimarySuccessionPlanRollup item)
    {
        return new PrimarySuccessionPlanRollupView(
            item.Code,
            item.Label,
            item.Channel,
            $"{item.Active}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimarySuccessionPlanRollupView> ToPrimarySuccessionPlanRollupViews(List<PrimarySuccessionPlanRollup> items)
    {
        var views = new List<PrimarySuccessionPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPrimarySuccessionPlanRollupView(item));
        }
        return views;
    }

    public static double SumPrimarySuccessionPlanRollupView(PrimarySuccessionPlanRollupView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMinimum + (double)parsedQuantity;
    }
}
