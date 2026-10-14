namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched adjustment schedule.</summary>
public sealed record BatchedAdjustmentScheduleView(string Code, string LabelText, string ExpiresAt, string MinimumText, string CapacityText, string Description, string StatusLabel)
{
    public static BatchedAdjustmentScheduleView ToBatchedAdjustmentScheduleView(BatchedAdjustmentSchedule item)
    {
        return new BatchedAdjustmentScheduleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAdjustmentScheduleView> ToBatchedAdjustmentScheduleViews(List<BatchedAdjustmentSchedule> items)
    {
        var views = new List<BatchedAdjustmentScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAdjustmentScheduleView(item));
        }
        return views;
    }

    public static double SumBatchedAdjustmentScheduleView(BatchedAdjustmentScheduleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedMinimum + (double)parsedCapacity;
    }
}
