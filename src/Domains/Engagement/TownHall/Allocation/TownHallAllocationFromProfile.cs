namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Functions of town hall allocation from profile.</summary>
public static class TownHallAllocationFromProfile
{
    public static Dictionary<string, double> SummarizeTownHallProfilesForAllocation(List<TownHallProfile> items)
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

    public static List<TownHallAllocation> DeriveTownHallAllocationsFromProfile(List<TownHallProfile> items)
    {
        var outValue = new List<TownHallAllocation>();
        foreach (var item in items)
        {
            var derived = TownHallAllocation.NewTownHallAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeTownHallAllocation(TownHallAllocation item)
    {
        return TownHallProfilePolicy.ClassifyTownHallProfile(item.NetExposure());
    }
}
