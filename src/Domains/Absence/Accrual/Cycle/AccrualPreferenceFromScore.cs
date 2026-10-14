namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Functions of accrual preference from score.</summary>
public static class AccrualPreferenceFromScore
{
    public static Dictionary<string, double> SummarizeLeaveRequestScoresForPreference(List<LeaveRequestScore> items)
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

    public static List<AccrualPreference> DeriveAccrualPreferencesFromScore(List<LeaveRequestScore> items)
    {
        var outValue = new List<AccrualPreference>();
        foreach (var item in items)
        {
            var derived = AccrualPreference.NewAccrualPreference(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveAccrualPreference(AccrualPreference item)
    {
        return LeaveRequestItemPolicy.NormalizeLeaveRequestItemCode(item.Code);
    }
}
