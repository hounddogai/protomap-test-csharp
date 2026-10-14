namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Functions of manual balance segment from slot.</summary>
public static class ManualBalanceSegmentFromSlot
{
    public static Dictionary<string, double> SummarizeReturnPlanSlotsForSegment(List<ReturnPlanSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ManualBalanceSegment> DeriveManualBalanceSegmentsFromSlot(List<ReturnPlanSlot> items)
    {
        var outValue = new List<ManualBalanceSegment>();
        foreach (var item in items)
        {
            var derived = ManualBalanceSegment.NewManualBalanceSegment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitManualBalanceSegment(ManualBalanceSegment item)
    {
        return ReturnPlanSlotPolicy.ClassifyReturnPlanSlot(item.WeightedExposure());
    }
}
