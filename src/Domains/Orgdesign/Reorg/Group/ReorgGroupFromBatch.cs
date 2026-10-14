namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Orgdesign.Position.RecordItem;
using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Functions of reorg group from batch.</summary>
public static class ReorgGroupFromBatch
{
    public static Dictionary<string, double> SummarizeScenarioBatchesForGroup(List<ScenarioBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ReorgGroup> DeriveReorgGroupsFromBatch(List<ScenarioBatch> items)
    {
        var outValue = new List<ReorgGroup>();
        foreach (var item in items)
        {
            var derived = ReorgGroup.NewReorgGroup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeReorgGroup(ReorgGroup item)
    {
        return ScenarioRequestPolicy.ScenarioRequestBucket(item.NetVolume());
    }

    public static Dictionary<string, double> SummarizePositionBatchesForGroup(List<PositionBatch> items)
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

    public static List<ReorgGroup> DeriveReorgGroupsFromBatchExtra(List<PositionBatch> items)
    {
        var outValue = new List<ReorgGroup>();
        foreach (var item in items)
        {
            var derived = ReorgGroup.NewReorgGroup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateReorgGroup(ReorgGroup item)
    {
        return PositionForecastPolicy.NormalizePositionForecastCode(item.Code);
    }
}
