namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Functions of lenient adjustment schedule from ledger.</summary>
public static class LenientAdjustmentScheduleFromLedger
{
    public static Dictionary<string, double> SummarizeEquityGrantLedgersForSchedule(List<EquityGrantLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<LenientAdjustmentSchedule> DeriveLenientAdjustmentSchedulesFromLedger(List<EquityGrantLedger> items)
    {
        var outValue = new List<LenientAdjustmentSchedule>();
        foreach (var item in items)
        {
            var derived = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitLenientAdjustmentSchedule(LenientAdjustmentSchedule item)
    {
        return EquityGrantLedgerPolicy.EquityGrantLedgerWeight(5, true);
    }
}
