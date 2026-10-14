namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Functions of merit cycle policy from profile.</summary>
public static class MeritCyclePolicyFromProfile
{
    public static Dictionary<string, double> SummarizeRollingBonusPoolProfilesForPolicy(List<RollingBonusPoolProfile> items)
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

    public static List<MeritCyclePolicy> DeriveMeritCyclePoliciesFromProfile(List<RollingBonusPoolProfile> items)
    {
        var outValue = new List<MeritCyclePolicy>();
        foreach (var item in items)
        {
            var derived = MeritCyclePolicy.NewMeritCyclePolicy(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceMeritCyclePolicy(MeritCyclePolicy item)
    {
        return DefaultBonusPoolReservationPolicy.DefaultBonusPoolReservationWeight(1, true);
    }
}
