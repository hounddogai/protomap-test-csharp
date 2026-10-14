namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Functions of policy override from reservation.</summary>
public static class PolicyOverrideFromReservation
{
    public static Dictionary<string, double> SummarizePolicyReservationsForOverride(List<PolicyReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PolicyOverride> DerivePolicyOverridesFromReservation(List<PolicyReservation> items)
    {
        var outValue = new List<PolicyOverride>();
        foreach (var item in items)
        {
            var derived = PolicyOverride.NewPolicyOverride(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPolicyOverride(PolicyOverride item)
    {
        return PolicyReservationPolicy.NormalizePolicyReservationCode(item.Code);
    }
}
