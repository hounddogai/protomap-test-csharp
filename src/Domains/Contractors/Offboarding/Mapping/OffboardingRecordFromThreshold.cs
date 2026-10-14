namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Functions of offboarding record from threshold.</summary>
public static class OffboardingRecordFromThreshold
{
    public static Dictionary<string, double> SummarizeLegacyOffboardingThresholdsForRecord(List<LegacyOffboardingThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<OffboardingRecord> DeriveOffboardingRecordsFromThreshold(List<LegacyOffboardingThreshold> items)
    {
        var outValue = new List<OffboardingRecord>();
        foreach (var item in items)
        {
            var derived = OffboardingRecord.NewOffboardingRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitOffboardingRecord(OffboardingRecord item)
    {
        return SeasonalOffboardingTierPolicy.SeasonalOffboardingTierWeight(2, true);
    }
}
