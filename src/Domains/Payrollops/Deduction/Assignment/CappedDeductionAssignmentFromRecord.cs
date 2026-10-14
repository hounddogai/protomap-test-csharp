namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Functions of capped deduction assignment from record.</summary>
public static class CappedDeductionAssignmentFromRecord
{
    public static Dictionary<string, double> SummarizeRetroPayRecordsForAssignment(List<RetroPayRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<CappedDeductionAssignment> DeriveCappedDeductionAssignmentsFromRecord(List<RetroPayRecord> items)
    {
        var outValue = new List<CappedDeductionAssignment>();
        foreach (var item in items)
        {
            var derived = CappedDeductionAssignment.NewCappedDeductionAssignment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankCappedDeductionAssignment(CappedDeductionAssignment item)
    {
        return RetroPayRecordPolicy.RetroPayRecordWeight(3, true);
    }
}
