namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Functions of tiered pool rule from reservation.</summary>
public static class TieredPoolRuleFromReservation
{
    public static Dictionary<string, double> SummarizeBatchedMobilityReservationsForRule(List<BatchedMobilityReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<TieredPoolRule> DeriveTieredPoolRulesFromReservation(List<BatchedMobilityReservation> items)
    {
        var outValue = new List<TieredPoolRule>();
        foreach (var item in items)
        {
            var derived = TieredPoolRule.NewTieredPoolRule(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeTieredPoolRule(TieredPoolRule item)
    {
        return MobilityEntryPolicy.ClassifyMobilityEntry(item.AdjustedVolume());
    }
}
