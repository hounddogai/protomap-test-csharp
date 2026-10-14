namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Functions of batched adjustment schedule from ledger.</summary>
public static class BatchedAdjustmentScheduleFromLedger
{
    public static Dictionary<string, double> SummarizeLenientEquityGrantLedgersForSchedule(List<LenientEquityGrantLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<BatchedAdjustmentSchedule> DeriveBatchedAdjustmentSchedulesFromLedger(List<LenientEquityGrantLedger> items)
    {
        var outValue = new List<BatchedAdjustmentSchedule>();
        foreach (var item in items)
        {
            var derived = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBatchedAdjustmentSchedule(BatchedAdjustmentSchedule item)
    {
        return EquityGrantRecordPolicy.NormalizeEquityGrantRecordCode(item.Code);
    }
}
