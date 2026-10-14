namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Functions of batched garnishment exception from variance.</summary>
public static class BatchedGarnishmentExceptionFromVariance
{
    public static Dictionary<string, double> SummarizeWeightedGarnishmentVariancesForException(List<WeightedGarnishmentVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<BatchedGarnishmentException> DeriveBatchedGarnishmentExceptionsFromVariance(List<WeightedGarnishmentVariance> items)
    {
        var outValue = new List<BatchedGarnishmentException>();
        foreach (var item in items)
        {
            var derived = BatchedGarnishmentException.NewBatchedGarnishmentException(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyBatchedGarnishmentException(BatchedGarnishmentException item)
    {
        return LegacyGarnishmentSlotPolicy.LegacyGarnishmentSlotBucket(item.NetLoad());
    }
}
