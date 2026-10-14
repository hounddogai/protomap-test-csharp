namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Functions of global balance policy from profile.</summary>
public static class GlobalBalancePolicyFromProfile
{
    public static Dictionary<string, double> SummarizeDefaultHolidayProfilesForPolicy(List<DefaultHolidayProfile> items)
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

    public static List<GlobalBalancePolicy> DeriveGlobalBalancePoliciesFromProfile(List<DefaultHolidayProfile> items)
    {
        var outValue = new List<GlobalBalancePolicy>();
        foreach (var item in items)
        {
            var derived = GlobalBalancePolicy.NewGlobalBalancePolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyGlobalBalancePolicy(GlobalBalancePolicy item)
    {
        return DefaultHolidayProfilePolicy.DefaultHolidayProfileWeight(5, true);
    }
}
