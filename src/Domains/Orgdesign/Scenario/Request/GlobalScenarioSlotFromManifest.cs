namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Functions of global scenario slot from manifest.</summary>
public static class GlobalScenarioSlotFromManifest
{
    public static Dictionary<string, double> SummarizeCostModelManifestsForSlot(List<CostModelManifest> items)
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

    public static List<GlobalScenarioSlot> DeriveGlobalScenarioSlotsFromManifest(List<CostModelManifest> items)
    {
        var outValue = new List<GlobalScenarioSlot>();
        foreach (var item in items)
        {
            var derived = GlobalScenarioSlot.NewGlobalScenarioSlot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateGlobalScenarioSlot(GlobalScenarioSlot item)
    {
        return CostModelSnapshotPolicy.NormalizeCostModelSnapshotCode(item.Code);
    }
}
