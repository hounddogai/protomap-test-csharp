namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched cost model batch.</summary>
public sealed record BatchedCostModelBatchView(string Code, string Title, string PhoneText, string Budget, string Visible, string CapacityText, string EndsAt, string Department, string Summary, string StatusLabel)
{
    public static BatchedCostModelBatchView ToBatchedCostModelBatchView(BatchedCostModelBatch item)
    {
        return new BatchedCostModelBatchView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            item.Phone,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            Text.Truncate(item.Summary, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedCostModelBatchView> ToBatchedCostModelBatchViews(List<BatchedCostModelBatch> items)
    {
        var views = new List<BatchedCostModelBatchView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedCostModelBatchView(item));
        }
        return views;
    }

    public static double SumBatchedCostModelBatchView(BatchedCostModelBatchView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedCapacity + (double)parsedEndsAt;
    }
}
