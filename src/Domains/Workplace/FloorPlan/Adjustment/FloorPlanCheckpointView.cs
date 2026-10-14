namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a floor plan checkpoint.</summary>
public sealed record FloorPlanCheckpointView(string Code, string Label, string SegmentText, string Description, string Version, string UpdatedAt, string Weight, string StatusLabel)
{
    public static FloorPlanCheckpointView ToFloorPlanCheckpointView(FloorPlanCheckpoint item)
    {
        return new FloorPlanCheckpointView(
            item.Code,
            item.Label,
            item.Segment,
            item.Description,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanCheckpointView> ToFloorPlanCheckpointViews(List<FloorPlanCheckpoint> items)
    {
        var views = new List<FloorPlanCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanCheckpointView(item));
        }
        return views;
    }

    public static double SumFloorPlanCheckpointView(FloorPlanCheckpointView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedUpdatedAt + parsedWeight;
    }
}
