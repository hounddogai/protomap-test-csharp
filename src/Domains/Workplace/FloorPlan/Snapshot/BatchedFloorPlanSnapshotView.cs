namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched floor plan snapshot.</summary>
public sealed record BatchedFloorPlanSnapshotView(string Code, string Name, string WeightText, string FloorValue, string ThresholdText, string BatchSizeText, string ContactEmailText, string EmployeeIdText, string GraceDays, string CountryText, string StatusLabel)
{
    public static BatchedFloorPlanSnapshotView ToBatchedFloorPlanSnapshotView(BatchedFloorPlanSnapshot item)
    {
        return new BatchedFloorPlanSnapshotView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            Text.Truncate(item.EmployeeId, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedFloorPlanSnapshotView> ToBatchedFloorPlanSnapshotViews(List<BatchedFloorPlanSnapshot> items)
    {
        var views = new List<BatchedFloorPlanSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedFloorPlanSnapshotView(item));
        }
        return views;
    }

    public static double SumBatchedFloorPlanSnapshotView(BatchedFloorPlanSnapshotView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedFloorValue + parsedThreshold;
    }
}
