namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Functions of batched nomination forecast from target.</summary>
public static class BatchedNominationForecastFromTarget
{
    public static Dictionary<string, double> SummarizePendingSuccessionPlanTargetsForForecast(List<PendingSuccessionPlanTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<BatchedNominationForecast> DeriveBatchedNominationForecastsFromTarget(List<PendingSuccessionPlanTarget> items)
    {
        var outValue = new List<BatchedNominationForecast>();
        foreach (var item in items)
        {
            var derived = BatchedNominationForecast.NewBatchedNominationForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignBatchedNominationForecast(BatchedNominationForecast item)
    {
        return SuccessionPlanProposalPolicy.SuccessionPlanProposalBucket(item.AdjustedVolume());
    }
}
