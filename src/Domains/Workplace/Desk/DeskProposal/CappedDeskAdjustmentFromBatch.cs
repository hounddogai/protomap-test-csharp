namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

/// <summary>Functions of capped desk adjustment from batch.</summary>
public static class CappedDeskAdjustmentFromBatch
{
    public static Dictionary<string, double> SummarizeStrictOccupancyBatchesForAdjustment(List<StrictOccupancyBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<CappedDeskAdjustment> DeriveCappedDeskAdjustmentsFromBatch(List<StrictOccupancyBatch> items)
    {
        var outValue = new List<CappedDeskAdjustment>();
        foreach (var item in items)
        {
            var derived = CappedDeskAdjustment.NewCappedDeskAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignCappedDeskAdjustment(CappedDeskAdjustment item)
    {
        return StrictOccupancyBatchPolicy.StrictOccupancyBatchWeight(7, true);
    }
}
