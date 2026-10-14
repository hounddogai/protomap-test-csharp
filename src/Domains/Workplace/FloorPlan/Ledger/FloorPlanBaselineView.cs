namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan baseline.</summary>
public sealed record FloorPlanBaselineView(string CodeText, string Name, string ScoreText, string StartsAt, string MinimumText, string SalaryBand, string Weight, string Discount, string Rating, string StatusLabel)
{
    public static FloorPlanBaselineView ToFloorPlanBaselineView(FloorPlanBaseline item)
    {
        return new FloorPlanBaselineView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanBaselineView> ToFloorPlanBaselineViews(List<FloorPlanBaseline> items)
    {
        var views = new List<FloorPlanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanBaselineView(item));
        }
        return views;
    }

    public static double SumFloorPlanBaselineView(FloorPlanBaselineView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedStartsAt + parsedMinimum;
    }
}
