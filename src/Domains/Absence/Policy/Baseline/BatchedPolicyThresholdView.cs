namespace Acme.Workforce.Absence.Policy.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched policy threshold.</summary>
public sealed record BatchedPolicyThresholdView(string CodeText, string LabelText, string ThresholdText, string FloorValue, string Capacity, string ContactEmail, string Priority, string Name, string StatusLabel)
{
    public static BatchedPolicyThresholdView ToBatchedPolicyThresholdView(BatchedPolicyThreshold item)
    {
        return new BatchedPolicyThresholdView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPolicyThresholdView> ToBatchedPolicyThresholdViews(List<BatchedPolicyThreshold> items)
    {
        var views = new List<BatchedPolicyThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPolicyThresholdView(item));
        }
        return views;
    }

    public static double SumBatchedPolicyThresholdView(BatchedPolicyThresholdView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedFloorValue + (double)parsedCapacity;
    }
}
