namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default floor plan baseline.</summary>
public sealed record DefaultFloorPlanBaselineView(string Code, string LabelText, string RetriesText, string Notes, string AmountText, string RatioText, string Location, string Archived, string StatusLabel)
{
    public static DefaultFloorPlanBaselineView ToDefaultFloorPlanBaselineView(DefaultFloorPlanBaseline item)
    {
        return new DefaultFloorPlanBaselineView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultFloorPlanBaselineView> ToDefaultFloorPlanBaselineViews(List<DefaultFloorPlanBaseline> items)
    {
        var views = new List<DefaultFloorPlanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultFloorPlanBaselineView(item));
        }
        return views;
    }

    public static double SumDefaultFloorPlanBaselineView(DefaultFloorPlanBaselineView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount + parsedRatio;
    }
}
