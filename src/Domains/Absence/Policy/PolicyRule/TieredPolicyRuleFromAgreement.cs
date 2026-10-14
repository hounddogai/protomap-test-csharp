namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Functions of tiered policy rule from agreement.</summary>
public static class TieredPolicyRuleFromAgreement
{
    public static Dictionary<string, double> SummarizePrimaryLeaveRequestAgreementsForRule(List<PrimaryLeaveRequestAgreement> items)
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

    public static List<TieredPolicyRule> DeriveTieredPolicyRulesFromAgreement(List<PrimaryLeaveRequestAgreement> items)
    {
        var outValue = new List<TieredPolicyRule>();
        foreach (var item in items)
        {
            var derived = TieredPolicyRule.NewTieredPolicyRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanTieredPolicyRule(TieredPolicyRule item)
    {
        return ManualLeaveRequestReservationPolicy.ManualLeaveRequestReservationBucket(item.ProjectedValue());
    }
}
