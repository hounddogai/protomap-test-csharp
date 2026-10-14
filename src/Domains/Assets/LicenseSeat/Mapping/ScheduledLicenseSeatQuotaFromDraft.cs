namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Functions of scheduled license seat quota from draft.</summary>
public static class ScheduledLicenseSeatQuotaFromDraft
{
    public static Dictionary<string, double> SummarizeFallbackAssetReturnDraftsForQuota(List<FallbackAssetReturnDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<ScheduledLicenseSeatQuota> DeriveScheduledLicenseSeatQuotasFromDraft(List<FallbackAssetReturnDraft> items)
    {
        var outValue = new List<ScheduledLicenseSeatQuota>();
        foreach (var item in items)
        {
            var derived = ScheduledLicenseSeatQuota.NewScheduledLicenseSeatQuota(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignScheduledLicenseSeatQuota(ScheduledLicenseSeatQuota item)
    {
        return RollingAssetReturnPlanPolicy.RollingAssetReturnPlanWeight(9, true);
    }
}
