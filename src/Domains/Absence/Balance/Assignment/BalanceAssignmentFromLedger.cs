namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Functions of balance assignment from ledger.</summary>
public static class BalanceAssignmentFromLedger
{
    public static Dictionary<string, double> SummarizeReturnPlanLedgersForAssignment(List<ReturnPlanLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<BalanceAssignment> DeriveBalanceAssignmentsFromLedger(List<ReturnPlanLedger> items)
    {
        var outValue = new List<BalanceAssignment>();
        foreach (var item in items)
        {
            var derived = BalanceAssignment.NewBalanceAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBalanceAssignment(BalanceAssignment item)
    {
        return ReturnPlanLedgerPolicy.ClassifyReturnPlanLedger(item.EffectiveExposure());
    }
}
