namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Functions of visitor cycle from ledger.</summary>
public static class VisitorCycleFromLedger
{
    public static Dictionary<string, double> SummarizeBatchedBookingLedgersForCycle(List<BatchedBookingLedger> items)
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

    public static List<VisitorCycle> DeriveVisitorCyclesFromLedger(List<BatchedBookingLedger> items)
    {
        var outValue = new List<VisitorCycle>();
        foreach (var item in items)
        {
            var derived = VisitorCycle.NewVisitorCycle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveVisitorCycle(VisitorCycle item)
    {
        return BatchedBookingLedgerPolicy.BatchedBookingLedgerWeight(8, true);
    }
}
