namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Functions of rolling merit cycle record from assignment.</summary>
public static class RollingMeritCycleRecordFromAssignment
{
    public static Dictionary<string, double> SummarizeGlobalEquityGrantAssignmentsForRecord(List<GlobalEquityGrantAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<RollingMeritCycleRecord> DeriveRollingMeritCycleRecordsFromAssignment(List<GlobalEquityGrantAssignment> items)
    {
        var outValue = new List<RollingMeritCycleRecord>();
        foreach (var item in items)
        {
            var derived = RollingMeritCycleRecord.NewRollingMeritCycleRecord(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateRollingMeritCycleRecord(RollingMeritCycleRecord item)
    {
        return EquityGrantVariancePolicy.ClassifyEquityGrantVariance(item.WeightedValue());
    }
}
