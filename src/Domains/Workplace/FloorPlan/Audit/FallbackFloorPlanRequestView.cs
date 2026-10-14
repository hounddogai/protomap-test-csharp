namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback floor plan request.</summary>
public sealed record FallbackFloorPlanRequestView(string Code, string LabelText, string LeaveDays, string Weight, string TitleText, string StatusLabel)
{
    public static FallbackFloorPlanRequestView ToFallbackFloorPlanRequestView(FallbackFloorPlanRequest item)
    {
        return new FallbackFloorPlanRequestView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackFloorPlanRequestView> ToFallbackFloorPlanRequestViews(List<FallbackFloorPlanRequest> items)
    {
        var views = new List<FallbackFloorPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackFloorPlanRequestView(item));
        }
        return views;
    }

    public static double SumFallbackFloorPlanRequestView(FallbackFloorPlanRequestView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedWeight;
    }
}
