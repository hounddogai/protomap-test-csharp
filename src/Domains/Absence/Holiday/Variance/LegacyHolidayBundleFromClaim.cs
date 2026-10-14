namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Functions of legacy holiday bundle from claim.</summary>
public static class LegacyHolidayBundleFromClaim
{
    public static Dictionary<string, double> SummarizeLeaveRequestClaimsForBundle(List<LeaveRequestClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<LegacyHolidayBundle> DeriveLegacyHolidayBundlesFromClaim(List<LeaveRequestClaim> items)
    {
        var outValue = new List<LegacyHolidayBundle>();
        foreach (var item in items)
        {
            var derived = LegacyHolidayBundle.NewLegacyHolidayBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLegacyHolidayBundle(LegacyHolidayBundle item)
    {
        return LeaveRequestClaimPolicy.LeaveRequestClaimBucket(item.ProjectedExposure());
    }
}
