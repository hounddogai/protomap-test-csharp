namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Functions of seasonal nomination cycle from record.</summary>
public static class SeasonalNominationCycleFromRecord
{
    public static Dictionary<string, double> SummarizeWeightedMobilityRecordsForCycle(List<WeightedMobilityRecord> items)
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

    public static List<SeasonalNominationCycle> DeriveSeasonalNominationCyclesFromRecord(List<WeightedMobilityRecord> items)
    {
        var outValue = new List<SeasonalNominationCycle>();
        foreach (var item in items)
        {
            var derived = SeasonalNominationCycle.NewSeasonalNominationCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceSeasonalNominationCycle(SeasonalNominationCycle item)
    {
        return CappedMobilityNoticePolicy.NormalizeCappedMobilityNoticeCode(item.Code);
    }
}
