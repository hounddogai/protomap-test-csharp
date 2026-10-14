namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Functions of benchmark request from record.</summary>
public static class BenchmarkRequestFromRecord
{
    public static Dictionary<string, double> SummarizeWeightedEquityGrantRecordsForRequest(List<WeightedEquityGrantRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<BenchmarkRequest> DeriveBenchmarkRequestsFromRecord(List<WeightedEquityGrantRecord> items)
    {
        var outValue = new List<BenchmarkRequest>();
        foreach (var item in items)
        {
            var derived = BenchmarkRequest.NewBenchmarkRequest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyBenchmarkRequest(BenchmarkRequest item)
    {
        return EquityGrantVariancePolicy.ClassifyEquityGrantVariance(item.MeasuredValue());
    }
}
