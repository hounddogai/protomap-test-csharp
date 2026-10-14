namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a floor plan review.</summary>
public sealed record FloorPlanReviewView(string CodeText, string NameText, string Ratio, string Threshold, string Ceiling, string Category, string StatusLabel)
{
    public static FloorPlanReviewView ToFloorPlanReviewView(FloorPlanReview item)
    {
        return new FloorPlanReviewView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanReviewView> ToFloorPlanReviewViews(List<FloorPlanReview> items)
    {
        var views = new List<FloorPlanReviewView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanReviewView(item));
        }
        return views;
    }

    public static double SumFloorPlanReviewView(FloorPlanReviewView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRatio + parsedThreshold + parsedCeiling;
    }
}
