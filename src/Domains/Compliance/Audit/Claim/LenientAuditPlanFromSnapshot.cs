namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Functions of lenient audit plan from snapshot.</summary>
public static class LenientAuditPlanFromSnapshot
{
    public static Dictionary<string, double> SummarizePolicyAckSnapshotsForPlan(List<PolicyAckSnapshot> items)
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

    public static List<LenientAuditPlan> DeriveLenientAuditPlansFromSnapshot(List<PolicyAckSnapshot> items)
    {
        var outValue = new List<LenientAuditPlan>();
        foreach (var item in items)
        {
            var derived = LenientAuditPlan.NewLenientAuditPlan(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignLenientAuditPlan(LenientAuditPlan item)
    {
        return PolicyAckReservationPolicy.PolicyAckReservationWeight(7, true);
    }
}
