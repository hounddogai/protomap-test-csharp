namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Functions of strict policy preference from digest.</summary>
public static class StrictPolicyPreferenceFromDigest
{
    public static Dictionary<string, double> SummarizeManualAccrualDigestsForPreference(List<ManualAccrualDigest> items)
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

    public static List<StrictPolicyPreference> DeriveStrictPolicyPreferencesFromDigest(List<ManualAccrualDigest> items)
    {
        var outValue = new List<StrictPolicyPreference>();
        foreach (var item in items)
        {
            var derived = StrictPolicyPreference.NewStrictPolicyPreference(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceStrictPolicyPreference(StrictPolicyPreference item)
    {
        return AccrualTargetPolicy.NormalizeAccrualTargetCode(item.Code);
    }
}
