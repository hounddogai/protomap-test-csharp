namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Functions of equity grant ledger from summary.</summary>
public static class EquityGrantLedgerFromSummary
{
    public static Dictionary<string, double> SummarizeBenchmarkSummariesForLedger(List<BenchmarkSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<EquityGrantLedger> DeriveEquityGrantLedgersFromSummary(List<BenchmarkSummary> items)
    {
        var outValue = new List<EquityGrantLedger>();
        foreach (var item in items)
        {
            var derived = EquityGrantLedger.NewEquityGrantLedger(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeEquityGrantLedger(EquityGrantLedger item)
    {
        return BenchmarkReviewPolicy.BenchmarkReviewBucket(item.AdjustedTotal());
    }
}
