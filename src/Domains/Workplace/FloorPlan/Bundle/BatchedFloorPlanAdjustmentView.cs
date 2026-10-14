namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched floor plan adjustment.</summary>
public sealed record BatchedFloorPlanAdjustmentView(string Code, string Name, string RateText, string StartsAt, string Reviewer, string CreatedAt, string RatioText, string Notes, string Team, string StatusLabel)
{
    public static BatchedFloorPlanAdjustmentView ToBatchedFloorPlanAdjustmentView(BatchedFloorPlanAdjustment item)
    {
        return new BatchedFloorPlanAdjustmentView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedFloorPlanAdjustmentView> ToBatchedFloorPlanAdjustmentViews(List<BatchedFloorPlanAdjustment> items)
    {
        var views = new List<BatchedFloorPlanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedFloorPlanAdjustmentView(item));
        }
        return views;
    }

    public static double SumBatchedFloorPlanAdjustmentView(BatchedFloorPlanAdjustmentView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedStartsAt + (double)parsedCreatedAt;
    }
}
