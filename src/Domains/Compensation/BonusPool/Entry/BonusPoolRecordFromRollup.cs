namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Functions of bonus pool record from rollup.</summary>
public static class BonusPoolRecordFromRollup
{
    public static Dictionary<string, double> SummarizeEquityGrantRollupsForRecord(List<EquityGrantRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BonusPoolRecord> DeriveBonusPoolRecordsFromRollup(List<EquityGrantRollup> items)
    {
        var outValue = new List<BonusPoolRecord>();
        foreach (var item in items)
        {
            var derived = BonusPoolRecord.NewBonusPoolRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeBonusPoolRecord(BonusPoolRecord item)
    {
        return EquityGrantOverridePolicy.ClassifyEquityGrantOverride(item.EffectiveLoad());
    }
}
