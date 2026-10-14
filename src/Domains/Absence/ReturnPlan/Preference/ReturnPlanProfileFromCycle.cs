namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Absence.Balance.Tier;

/// <summary>Functions of return plan profile from cycle.</summary>
public static class ReturnPlanProfileFromCycle
{
    public static Dictionary<string, double> SummarizeBalanceCyclesForProfile(List<BalanceCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<ReturnPlanProfile> DeriveReturnPlanProfilesFromCycle(List<BalanceCycle> items)
    {
        var outValue = new List<ReturnPlanProfile>();
        foreach (var item in items)
        {
            var derived = ReturnPlanProfile.NewReturnPlanProfile(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitReturnPlanProfile(ReturnPlanProfile item)
    {
        return BalanceCyclePolicy.NormalizeBalanceCycleCode(item.Code);
    }
}
