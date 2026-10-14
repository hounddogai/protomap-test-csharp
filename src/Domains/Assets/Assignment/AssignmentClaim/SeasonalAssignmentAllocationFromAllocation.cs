namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Functions of seasonal assignment allocation from allocation.</summary>
public static class SeasonalAssignmentAllocationFromAllocation
{
    public static Dictionary<string, double> SummarizeRefreshAllocationsForAllocation(List<RefreshAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<SeasonalAssignmentAllocation> DeriveSeasonalAssignmentAllocationsFromAllocation(List<RefreshAllocation> items)
    {
        var outValue = new List<SeasonalAssignmentAllocation>();
        foreach (var item in items)
        {
            var derived = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeSeasonalAssignmentAllocation(SeasonalAssignmentAllocation item)
    {
        return RefreshAllocationPolicy.NormalizeRefreshAllocationCode(item.Code);
    }
}
