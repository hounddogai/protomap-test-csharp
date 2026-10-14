namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Functions of seasonal assignment allocation from exception.</summary>
public static class SeasonalAssignmentAllocationFromException
{
    public static Dictionary<string, double> SummarizeLicenseSeatExceptionsForAllocation(List<LicenseSeatException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalAssignmentAllocation> DeriveSeasonalAssignmentAllocationsFromException(List<LicenseSeatException> items)
    {
        var outValue = new List<SeasonalAssignmentAllocation>();
        foreach (var item in items)
        {
            var derived = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeSeasonalAssignmentAllocationExtra(SeasonalAssignmentAllocation item)
    {
        return LicenseSeatExceptionPolicy.LicenseSeatExceptionBucket(item.WeightedExposure());
    }
}
