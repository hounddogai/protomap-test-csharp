namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Functions of policy reservation from item.</summary>
public static class PolicyReservationFromItem
{
    public static Dictionary<string, double> SummarizePolicyItemsForReservation(List<PolicyItem> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PolicyReservation> DerivePolicyReservationsFromItem(List<PolicyItem> items)
    {
        var outValue = new List<PolicyReservation>();
        foreach (var item in items)
        {
            var derived = PolicyReservation.NewPolicyReservation(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScorePolicyReservation(PolicyReservation item)
    {
        return PolicyAdjustmentPolicy.PolicyAdjustmentWeight(5, true);
    }
}
