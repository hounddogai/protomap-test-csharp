namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Functions of global mobility record from audit.</summary>
public static class GlobalMobilityRecordFromAudit
{
    public static Dictionary<string, double> SummarizeCareerPathAuditsForRecord(List<CareerPathAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<GlobalMobilityRecord> DeriveGlobalMobilityRecordsFromAudit(List<CareerPathAudit> items)
    {
        var outValue = new List<GlobalMobilityRecord>();
        foreach (var item in items)
        {
            var derived = GlobalMobilityRecord.NewGlobalMobilityRecord(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeGlobalMobilityRecord(GlobalMobilityRecord item)
    {
        return CareerPathAuditPolicy.NormalizeCareerPathAuditCode(item.Code);
    }
}
