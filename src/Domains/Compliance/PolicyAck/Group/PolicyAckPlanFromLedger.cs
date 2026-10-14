namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Functions of policy ack plan from ledger.</summary>
public static class PolicyAckPlanFromLedger
{
    public static Dictionary<string, double> SummarizeScheduledExceptionLedgersForPlan(List<ScheduledExceptionLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PolicyAckPlan> DerivePolicyAckPlansFromLedger(List<ScheduledExceptionLedger> items)
    {
        var outValue = new List<PolicyAckPlan>();
        foreach (var item in items)
        {
            var derived = PolicyAckPlan.NewPolicyAckPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPolicyAckPlan(PolicyAckPlan item)
    {
        return ExceptionAgreementPolicy.NormalizeExceptionAgreementCode(item.Code);
    }
}
