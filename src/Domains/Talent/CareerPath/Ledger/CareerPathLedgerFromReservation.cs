namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Functions of career path ledger from reservation.</summary>
public static class CareerPathLedgerFromReservation
{
    public static Dictionary<string, double> SummarizeBatchedMobilityReservationsForLedger(List<BatchedMobilityReservation> items)
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

    public static List<CareerPathLedger> DeriveCareerPathLedgersFromReservation(List<BatchedMobilityReservation> items)
    {
        var outValue = new List<CareerPathLedger>();
        foreach (var item in items)
        {
            var derived = CareerPathLedger.NewCareerPathLedger(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileCareerPathLedger(CareerPathLedger item)
    {
        return MobilityEntryPolicy.MobilityEntryBucket(item.AdjustedTotal());
    }
}
