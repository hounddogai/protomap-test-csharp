namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Functions of retro pay ledger from allocation.</summary>
public static class RetroPayLedgerFromAllocation
{
    public static Dictionary<string, double> SummarizePrimaryGarnishmentAllocationsForLedger(List<PrimaryGarnishmentAllocation> items)
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

    public static List<RetroPayLedger> DeriveRetroPayLedgersFromAllocation(List<PrimaryGarnishmentAllocation> items)
    {
        var outValue = new List<RetroPayLedger>();
        foreach (var item in items)
        {
            var derived = RetroPayLedger.NewRetroPayLedger(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeRetroPayLedger(RetroPayLedger item)
    {
        return LegacyGarnishmentSlotPolicy.LegacyGarnishmentSlotBucket(item.EffectiveLoad());
    }
}
