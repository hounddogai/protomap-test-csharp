namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive policy allocation.</summary>
public sealed record AdaptivePolicyAllocationView(string CodeText, string Label, string Capacity, string Quantity, string BatchSize, string Weight, string Priority, string RateText, string UpdatedAt, string Ceiling, string Unit, string StatusLabel)
{
    public static AdaptivePolicyAllocationView ToAdaptivePolicyAllocationView(AdaptivePolicyAllocation item)
    {
        return new AdaptivePolicyAllocationView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePolicyAllocationView> ToAdaptivePolicyAllocationViews(List<AdaptivePolicyAllocation> items)
    {
        var views = new List<AdaptivePolicyAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePolicyAllocationView(item));
        }
        return views;
    }

    public static double SumAdaptivePolicyAllocationView(AdaptivePolicyAllocationView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedQuantity + (double)parsedBatchSize;
    }
}
