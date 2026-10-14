namespace Acme.Workforce.Talent.Pool.PoolDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched pool audit.</summary>
public sealed record BatchedPoolAuditView(string Code, string Name, string LimitText, string CostCenter, string Capacity, string StatusLabel)
{
    public static BatchedPoolAuditView ToBatchedPoolAuditView(BatchedPoolAudit item)
    {
        return new BatchedPoolAuditView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPoolAuditView> ToBatchedPoolAuditViews(List<BatchedPoolAudit> items)
    {
        var views = new List<BatchedPoolAuditView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPoolAuditView(item));
        }
        return views;
    }

    public static double SumBatchedPoolAuditView(BatchedPoolAuditView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedCapacity;
    }
}
