namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Functions of license seat milestone from batch.</summary>
public static class LicenseSeatMilestoneFromBatch
{
    public static Dictionary<string, double> SummarizeRefreshBatchesForMilestone(List<RefreshBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<LicenseSeatMilestone> DeriveLicenseSeatMilestonesFromBatch(List<RefreshBatch> items)
    {
        var outValue = new List<LicenseSeatMilestone>();
        foreach (var item in items)
        {
            var derived = LicenseSeatMilestone.NewLicenseSeatMilestone(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreLicenseSeatMilestone(LicenseSeatMilestone item)
    {
        return RefreshBatchPolicy.RefreshBatchWeight(1, true);
    }
}
