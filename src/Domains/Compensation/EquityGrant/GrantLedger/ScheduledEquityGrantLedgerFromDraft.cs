namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Functions of scheduled equity grant ledger from draft.</summary>
public static class ScheduledEquityGrantLedgerFromDraft
{
    public static Dictionary<string, double> SummarizeManualBonusPoolDraftsForLedger(List<ManualBonusPoolDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<ScheduledEquityGrantLedger> DeriveScheduledEquityGrantLedgersFromDraft(List<ManualBonusPoolDraft> items)
    {
        var outValue = new List<ScheduledEquityGrantLedger>();
        foreach (var item in items)
        {
            var derived = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateScheduledEquityGrantLedger(ScheduledEquityGrantLedger item)
    {
        return BonusPoolRollupPolicy.NormalizeBonusPoolRollupCode(item.Code);
    }
}
