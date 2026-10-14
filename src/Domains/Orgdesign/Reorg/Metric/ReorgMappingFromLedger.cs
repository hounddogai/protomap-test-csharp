namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Functions of reorg mapping from ledger.</summary>
public static class ReorgMappingFromLedger
{
    public static Dictionary<string, double> SummarizeCostModelLedgersForMapping(List<CostModelLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<ReorgMapping> DeriveReorgMappingsFromLedger(List<CostModelLedger> items)
    {
        var outValue = new List<ReorgMapping>();
        foreach (var item in items)
        {
            var derived = ReorgMapping.NewReorgMapping(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeReorgMapping(ReorgMapping item)
    {
        return ScheduledCostModelBatchPolicy.NormalizeScheduledCostModelBatchCode(item.Code);
    }
}
