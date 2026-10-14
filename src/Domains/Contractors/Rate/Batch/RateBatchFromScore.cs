namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Functions of rate batch from score.</summary>
public static class RateBatchFromScore
{
    public static Dictionary<string, double> SummarizeStatementOfWorkScoresForBatch(List<StatementOfWorkScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<RateBatch> DeriveRateBatchesFromScore(List<StatementOfWorkScore> items)
    {
        var outValue = new List<RateBatch>();
        foreach (var item in items)
        {
            var derived = RateBatch.NewRateBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyRateBatchExtra(RateBatch item)
    {
        return StatementOfWorkPolicyPolicy.StatementOfWorkPolicyBucket(item.AdjustedTotal());
    }
}
