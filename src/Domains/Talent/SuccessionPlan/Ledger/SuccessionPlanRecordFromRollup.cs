namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Functions of succession plan record from rollup.</summary>
public static class SuccessionPlanRecordFromRollup
{
    public static Dictionary<string, double> SummarizeMobilityRollupsForRecord(List<MobilityRollup> items)
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

    public static List<SuccessionPlanRecord> DeriveSuccessionPlanRecordsFromRollup(List<MobilityRollup> items)
    {
        var outValue = new List<SuccessionPlanRecord>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanRecord.NewSuccessionPlanRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleSuccessionPlanRecord(SuccessionPlanRecord item)
    {
        return MobilityScorePolicy.MobilityScoreBucket(item.NetExposure());
    }
}
