namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Functions of mobility allocation from record.</summary>
public static class MobilityAllocationFromRecord
{
    public static Dictionary<string, double> SummarizePrimarySuccessionPlanRecordsForAllocation(List<PrimarySuccessionPlanRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<MobilityAllocation> DeriveMobilityAllocationsFromRecord(List<PrimarySuccessionPlanRecord> items)
    {
        var outValue = new List<MobilityAllocation>();
        foreach (var item in items)
        {
            var derived = MobilityAllocation.NewMobilityAllocation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitMobilityAllocation(MobilityAllocation item)
    {
        return PrimarySuccessionPlanRecordPolicy.NormalizePrimarySuccessionPlanRecordCode(item.Code);
    }
}
