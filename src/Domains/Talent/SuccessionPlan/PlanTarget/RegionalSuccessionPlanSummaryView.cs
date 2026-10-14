namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional succession plan summary.</summary>
public sealed record RegionalSuccessionPlanSummaryView(string Code, string Label, string Rating, string CapacityText, string WindowDays, string JobCodeText, string Minimum, string QuantityText, string StatusLabel)
{
    public static RegionalSuccessionPlanSummaryView ToRegionalSuccessionPlanSummaryView(RegionalSuccessionPlanSummary item)
    {
        return new RegionalSuccessionPlanSummaryView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSuccessionPlanSummaryView> ToRegionalSuccessionPlanSummaryViews(List<RegionalSuccessionPlanSummary> items)
    {
        var views = new List<RegionalSuccessionPlanSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSuccessionPlanSummaryView(item));
        }
        return views;
    }

    public static double SumRegionalSuccessionPlanSummaryView(RegionalSuccessionPlanSummaryView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCapacity + (double)parsedWindowDays;
    }
}
