namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Functions of seasonal nomination ledger from milestone.</summary>
public static class SeasonalNominationLedgerFromMilestone
{
    public static Dictionary<string, double> SummarizeDefaultPoolMilestonesForLedger(List<DefaultPoolMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<SeasonalNominationLedger> DeriveSeasonalNominationLedgersFromMilestone(List<DefaultPoolMilestone> items)
    {
        var outValue = new List<SeasonalNominationLedger>();
        foreach (var item in items)
        {
            var derived = SeasonalNominationLedger.NewSeasonalNominationLedger(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveSeasonalNominationLedger(SeasonalNominationLedger item)
    {
        return DefaultPoolExceptionPolicy.DefaultPoolExceptionWeight(9, true);
    }
}
