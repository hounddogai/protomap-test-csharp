namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Draft;
using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Functions of adaptive laptop checkpoint from segment.</summary>
public static class AdaptiveLaptopCheckpointFromSegment
{
    public static Dictionary<string, double> SummarizeCappedLaptopSegmentsForCheckpoint(List<CappedLaptopSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<AdaptiveLaptopCheckpoint> DeriveAdaptiveLaptopCheckpointsFromSegment(List<CappedLaptopSegment> items)
    {
        var outValue = new List<AdaptiveLaptopCheckpoint>();
        foreach (var item in items)
        {
            var derived = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveAdaptiveLaptopCheckpoint(AdaptiveLaptopCheckpoint item)
    {
        return LaptopMappingPolicy.LaptopMappingWeight(1, true);
    }

    public static Dictionary<string, double> SummarizeRefreshSegmentsForCheckpoint(List<RefreshSegment> items)
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

    public static List<AdaptiveLaptopCheckpoint> DeriveAdaptiveLaptopCheckpointsFromSegmentExtra(List<RefreshSegment> items)
    {
        var outValue = new List<AdaptiveLaptopCheckpoint>();
        foreach (var item in items)
        {
            var derived = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAdaptiveLaptopCheckpointExtra(AdaptiveLaptopCheckpoint item)
    {
        return ScheduledRefreshCyclePolicy.NormalizeScheduledRefreshCycleCode(item.Code);
    }
}
