namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Functions of rate cycle from digest.</summary>
public static class RateCycleFromDigest
{
    public static Dictionary<string, double> SummarizeDefaultEngagementDigestsForCycle(List<DefaultEngagementDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RateCycle> DeriveRateCyclesFromDigest(List<DefaultEngagementDigest> items)
    {
        var outValue = new List<RateCycle>();
        foreach (var item in items)
        {
            var derived = RateCycle.NewRateCycle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRateCycle(RateCycle item)
    {
        return EngagementCheckpointPolicy.ClassifyEngagementCheckpoint(item.MeasuredValue());
    }
}
