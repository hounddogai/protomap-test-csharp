namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Functions of seasonal nomination ledger from proposal.</summary>
public static class SeasonalNominationLedgerFromProposal
{
    public static Dictionary<string, double> SummarizeCareerPathProposalsForLedger(List<CareerPathProposal> items)
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

    public static List<SeasonalNominationLedger> DeriveSeasonalNominationLedgersFromProposal(List<CareerPathProposal> items)
    {
        var outValue = new List<SeasonalNominationLedger>();
        foreach (var item in items)
        {
            var derived = SeasonalNominationLedger.NewSeasonalNominationLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateSeasonalNominationLedger(SeasonalNominationLedger item)
    {
        return ManualCareerPathEntryPolicy.ClassifyManualCareerPathEntry(item.EffectiveExposure());
    }
}
