namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Functions of pay band cycle from entry.</summary>
public static class PayBandCycleFromEntry
{
    public static Dictionary<string, double> SummarizeEquityGrantEntriesForCycle(List<EquityGrantEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PayBandCycle> DerivePayBandCyclesFromEntry(List<EquityGrantEntry> items)
    {
        var outValue = new List<PayBandCycle>();
        foreach (var item in items)
        {
            var derived = PayBandCycle.NewPayBandCycle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePayBandCycle(PayBandCycle item)
    {
        return EquityGrantSlotPolicy.EquityGrantSlotBucket(item.NetVolume());
    }
}
