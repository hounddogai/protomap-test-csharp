namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback action plan segment.</summary>
public sealed record FallbackActionPlanSegmentView(string Code, string Name, string FteText, string ThresholdText, string Segment, string BatchSize, string DepartmentText, string FloorValue, string Grade, string StatusLabel)
{
    public static FallbackActionPlanSegmentView ToFallbackActionPlanSegmentView(FallbackActionPlanSegment item)
    {
        return new FallbackActionPlanSegmentView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackActionPlanSegmentView> ToFallbackActionPlanSegmentViews(List<FallbackActionPlanSegment> items)
    {
        var views = new List<FallbackActionPlanSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackActionPlanSegmentView(item));
        }
        return views;
    }

    public static double SumFallbackActionPlanSegmentView(FallbackActionPlanSegmentView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedFte + parsedThreshold + (double)parsedBatchSize;
    }
}
