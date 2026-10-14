namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Functions of strict cost model checkpoint from ledger.</summary>
public static class StrictCostModelCheckpointFromLedger
{
    public static Dictionary<string, double> SummarizeCappedScenarioLedgersForCheckpoint(List<CappedScenarioLedger> items)
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

    public static List<StrictCostModelCheckpoint> DeriveStrictCostModelCheckpointsFromLedger(List<CappedScenarioLedger> items)
    {
        var outValue = new List<StrictCostModelCheckpoint>();
        foreach (var item in items)
        {
            var derived = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignStrictCostModelCheckpoint(StrictCostModelCheckpoint item)
    {
        return CappedScenarioLedgerPolicy.CappedScenarioLedgerWeight(1, true);
    }
}
