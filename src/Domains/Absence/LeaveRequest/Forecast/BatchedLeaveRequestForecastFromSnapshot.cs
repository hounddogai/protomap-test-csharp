namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Functions of batched leave request forecast from snapshot.</summary>
public static class BatchedLeaveRequestForecastFromSnapshot
{
    public static Dictionary<string, double> SummarizeScheduledLeaveRequestSnapshotsForForecast(List<ScheduledLeaveRequestSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<BatchedLeaveRequestForecast> DeriveBatchedLeaveRequestForecastsFromSnapshot(List<ScheduledLeaveRequestSnapshot> items)
    {
        var outValue = new List<BatchedLeaveRequestForecast>();
        foreach (var item in items)
        {
            var derived = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleBatchedLeaveRequestForecast(BatchedLeaveRequestForecast item)
    {
        return LeaveRequestDraftPolicy.NormalizeLeaveRequestDraftCode(item.Code);
    }
}
