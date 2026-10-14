namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Functions of holiday policy from assignment.</summary>
public static class HolidayPolicyFromAssignment
{
    public static Dictionary<string, double> SummarizeBalanceAssignmentsForPolicy(List<BalanceAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<HolidayPolicy> DeriveHolidayPoliciesFromAssignment(List<BalanceAssignment> items)
    {
        var outValue = new List<HolidayPolicy>();
        foreach (var item in items)
        {
            var derived = HolidayPolicy.NewHolidayPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyHolidayPolicy(HolidayPolicy item)
    {
        return BalanceDraftPolicy.BalanceDraftBucket(item.MeasuredValue());
    }
}
