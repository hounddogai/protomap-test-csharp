namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled cost model batch.</summary>
public sealed record ScheduledCostModelBatchView(string Code, string Name, string Approved, string ExpiresAt, string MarginText, string OwnerIdText, string ExternalRef, string Capacity, string StatusLabel)
{
    public static ScheduledCostModelBatchView ToScheduledCostModelBatchView(ScheduledCostModelBatch item)
    {
        return new ScheduledCostModelBatchView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Approved}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.ExternalRef,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledCostModelBatchView> ToScheduledCostModelBatchViews(List<ScheduledCostModelBatch> items)
    {
        var views = new List<ScheduledCostModelBatchView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledCostModelBatchView(item));
        }
        return views;
    }

    public static double SumScheduledCostModelBatchView(ScheduledCostModelBatchView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedMargin + (double)parsedCapacity;
    }
}
