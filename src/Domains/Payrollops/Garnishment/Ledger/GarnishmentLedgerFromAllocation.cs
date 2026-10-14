namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Functions of garnishment ledger from allocation.</summary>
public static class GarnishmentLedgerFromAllocation
{
    public static Dictionary<string, double> SummarizeGarnishmentAllocationsForLedger(List<GarnishmentAllocation> items)
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

    public static List<GarnishmentLedger> DeriveGarnishmentLedgersFromAllocation(List<GarnishmentAllocation> items)
    {
        var outValue = new List<GarnishmentLedger>();
        foreach (var item in items)
        {
            var derived = GarnishmentLedger.NewGarnishmentLedger(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignGarnishmentLedger(GarnishmentLedger item)
    {
        return GarnishmentAllocationPolicy.GarnishmentAllocationWeight(9, true);
    }
}
