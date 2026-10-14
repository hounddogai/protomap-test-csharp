namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Functions of legacy nomination rollup from tier.</summary>
public static class LegacyNominationRollupFromTier
{
    public static Dictionary<string, double> SummarizeStrictNominationTiersForRollup(List<StrictNominationTier> items)
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

    public static List<LegacyNominationRollup> DeriveLegacyNominationRollupsFromTier(List<StrictNominationTier> items)
    {
        var outValue = new List<LegacyNominationRollup>();
        foreach (var item in items)
        {
            var derived = LegacyNominationRollup.NewLegacyNominationRollup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLegacyNominationRollup(LegacyNominationRollup item)
    {
        return NominationPolicyPolicy.NominationPolicyBucket(item.AdjustedVolume());
    }
}
