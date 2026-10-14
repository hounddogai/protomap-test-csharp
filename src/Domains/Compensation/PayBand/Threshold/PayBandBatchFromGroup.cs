namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Functions of pay band batch from group.</summary>
public static class PayBandBatchFromGroup
{
    public static Dictionary<string, double> SummarizeBonusPoolGroupsForBatch(List<BonusPoolGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<PayBandBatch> DerivePayBandBatchesFromGroup(List<BonusPoolGroup> items)
    {
        var outValue = new List<PayBandBatch>();
        foreach (var item in items)
        {
            var derived = PayBandBatch.NewPayBandBatch(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePayBandBatch(PayBandBatch item)
    {
        return BonusPoolSchedulePolicy.ClassifyBonusPoolSchedule(item.AdjustedValue());
    }
}
