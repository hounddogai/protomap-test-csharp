namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Functions of policy ledger from mapping.</summary>
public static class PolicyLedgerFromMapping
{
    public static Dictionary<string, double> SummarizeHolidayMappingsForLedger(List<HolidayMapping> items)
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

    public static List<PolicyLedger> DerivePolicyLedgersFromMapping(List<HolidayMapping> items)
    {
        var outValue = new List<PolicyLedger>();
        foreach (var item in items)
        {
            var derived = PolicyLedger.NewPolicyLedger(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePolicyLedger(PolicyLedger item)
    {
        return HolidayReviewPolicy.NormalizeHolidayReviewCode(item.Code);
    }
}
