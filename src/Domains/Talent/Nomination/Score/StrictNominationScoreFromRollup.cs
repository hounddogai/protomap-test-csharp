namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Functions of strict nomination score from rollup.</summary>
public static class StrictNominationScoreFromRollup
{
    public static Dictionary<string, double> SummarizeWeightedMobilityRollupsForScore(List<WeightedMobilityRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<StrictNominationScore> DeriveStrictNominationScoresFromRollup(List<WeightedMobilityRollup> items)
    {
        var outValue = new List<StrictNominationScore>();
        foreach (var item in items)
        {
            var derived = StrictNominationScore.NewStrictNominationScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateStrictNominationScore(StrictNominationScore item)
    {
        return WeightedMobilityRollupPolicy.WeightedMobilityRollupWeight(1, true);
    }
}
