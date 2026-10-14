namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>Functions of nomination slot from allocation.</summary>
public static class NominationSlotFromAllocation
{
    public static Dictionary<string, double> SummarizeCareerPathAllocationsForSlot(List<CareerPathAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<NominationSlot> DeriveNominationSlotsFromAllocation(List<CareerPathAllocation> items)
    {
        var outValue = new List<NominationSlot>();
        foreach (var item in items)
        {
            var derived = NominationSlot.NewNominationSlot(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeNominationSlot(NominationSlot item)
    {
        return CareerPathAdjustmentPolicy.ClassifyCareerPathAdjustment(item.AdjustedTotal());
    }
}
