namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Functions of manual return plan ledger from schedule.</summary>
public static class ManualReturnPlanLedgerFromSchedule
{
    public static Dictionary<string, double> SummarizePolicySchedulesForLedger(List<PolicySchedule> items)
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

    public static List<ManualReturnPlanLedger> DeriveManualReturnPlanLedgersFromSchedule(List<PolicySchedule> items)
    {
        var outValue = new List<ManualReturnPlanLedger>();
        foreach (var item in items)
        {
            var derived = ManualReturnPlanLedger.NewManualReturnPlanLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitManualReturnPlanLedger(ManualReturnPlanLedger item)
    {
        return PrimaryPolicyItemPolicy.PrimaryPolicyItemWeight(7, true);
    }
}
