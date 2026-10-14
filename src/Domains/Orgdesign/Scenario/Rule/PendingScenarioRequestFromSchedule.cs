namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Functions of pending scenario request from schedule.</summary>
public static class PendingScenarioRequestFromSchedule
{
    public static Dictionary<string, double> SummarizeWeightedHeadcountSchedulesForRequest(List<WeightedHeadcountSchedule> items)
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

    public static List<PendingScenarioRequest> DerivePendingScenarioRequestsFromSchedule(List<WeightedHeadcountSchedule> items)
    {
        var outValue = new List<PendingScenarioRequest>();
        foreach (var item in items)
        {
            var derived = PendingScenarioRequest.NewPendingScenarioRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPendingScenarioRequest(PendingScenarioRequest item)
    {
        return HeadcountScorePolicy.HeadcountScoreBucket(item.WeightedLoad());
    }
}
