namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>Functions of fallback balance mapping from assignment.</summary>
public static class FallbackBalanceMappingFromAssignment
{
    public static Dictionary<string, double> SummarizeScheduledAccrualAssignmentsForMapping(List<ScheduledAccrualAssignment> items)
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

    public static List<FallbackBalanceMapping> DeriveFallbackBalanceMappingsFromAssignment(List<ScheduledAccrualAssignment> items)
    {
        var outValue = new List<FallbackBalanceMapping>();
        foreach (var item in items)
        {
            var derived = FallbackBalanceMapping.NewFallbackBalanceMapping(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeFallbackBalanceMapping(FallbackBalanceMapping item)
    {
        return ScheduledAccrualAssignmentPolicy.ScheduledAccrualAssignmentWeight(4, true);
    }
}
