namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>Functions of occupancy adjustment from cycle.</summary>
public static class OccupancyAdjustmentFromCycle
{
    public static Dictionary<string, double> SummarizeOccupancyCyclesForAdjustment(List<OccupancyCycle> items)
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

    public static List<OccupancyAdjustment> DeriveOccupancyAdjustmentsFromCycle(List<OccupancyCycle> items)
    {
        var outValue = new List<OccupancyAdjustment>();
        foreach (var item in items)
        {
            var derived = OccupancyAdjustment.NewOccupancyAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyOccupancyAdjustment(OccupancyAdjustment item)
    {
        return OccupancyLedgerPolicy.NormalizeOccupancyLedgerCode(item.Code);
    }
}
