namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched offboarding allocation.</summary>
public sealed record BatchedOffboardingAllocationView(string Code, string Label, string HoursText, string BatchSize, string Approved, string Margin, string Timezone, string StatusLabel)
{
    public static BatchedOffboardingAllocationView ToBatchedOffboardingAllocationView(BatchedOffboardingAllocation item)
    {
        return new BatchedOffboardingAllocationView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedOffboardingAllocationView> ToBatchedOffboardingAllocationViews(List<BatchedOffboardingAllocation> items)
    {
        var views = new List<BatchedOffboardingAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedOffboardingAllocationView(item));
        }
        return views;
    }

    public static double SumBatchedOffboardingAllocationView(BatchedOffboardingAllocationView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedBatchSize + parsedMargin;
    }
}
