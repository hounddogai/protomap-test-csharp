namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Functions of cost model record from plan.</summary>
public static class CostModelRecordFromPlan
{
    public static Dictionary<string, double> SummarizeLegacyPositionPlansForRecord(List<LegacyPositionPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<CostModelRecord> DeriveCostModelRecordsFromPlan(List<LegacyPositionPlan> items)
    {
        var outValue = new List<CostModelRecord>();
        foreach (var item in items)
        {
            var derived = CostModelRecord.NewCostModelRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreCostModelRecord(CostModelRecord item)
    {
        return RollingPositionManifestPolicy.NormalizeRollingPositionManifestCode(item.Code);
    }
}
