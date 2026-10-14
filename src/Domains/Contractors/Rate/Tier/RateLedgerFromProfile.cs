namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>Functions of rate ledger from profile.</summary>
public static class RateLedgerFromProfile
{
    public static Dictionary<string, double> SummarizeManualStatementOfWorkProfilesForLedger(List<ManualStatementOfWorkProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<RateLedger> DeriveRateLedgersFromProfile(List<ManualStatementOfWorkProfile> items)
    {
        var outValue = new List<RateLedger>();
        foreach (var item in items)
        {
            var derived = RateLedger.NewRateLedger(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveRateLedger(RateLedger item)
    {
        return ManualStatementOfWorkProfilePolicy.ManualStatementOfWorkProfileWeight(4, true);
    }
}
