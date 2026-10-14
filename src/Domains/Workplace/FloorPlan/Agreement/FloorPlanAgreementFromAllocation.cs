namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Functions of floor plan agreement from allocation.</summary>
public static class FloorPlanAgreementFromAllocation
{
    public static Dictionary<string, double> SummarizeDeskAllocationsForAgreement(List<DeskAllocation> items)
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

    public static List<FloorPlanAgreement> DeriveFloorPlanAgreementsFromAllocation(List<DeskAllocation> items)
    {
        var outValue = new List<FloorPlanAgreement>();
        foreach (var item in items)
        {
            var derived = FloorPlanAgreement.NewFloorPlanAgreement(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateFloorPlanAgreement(FloorPlanAgreement item)
    {
        return DeskAllocationPolicy.DeskAllocationWeight(3, true);
    }
}
