namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Functions of exception threshold from batch.</summary>
public static class ExceptionThresholdFromBatch
{
    public static Dictionary<string, double> SummarizeExceptionBatchesForThreshold(List<ExceptionBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<ExceptionThreshold> DeriveExceptionThresholdsFromBatch(List<ExceptionBatch> items)
    {
        var outValue = new List<ExceptionThreshold>();
        foreach (var item in items)
        {
            var derived = ExceptionThreshold.NewExceptionThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignExceptionThreshold(ExceptionThreshold item)
    {
        return CappedExceptionAllocationPolicy.CappedExceptionAllocationWeight(4, true);
    }
}
