namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Functions of lenient accrual override from group.</summary>
public static class LenientAccrualOverrideFromGroup
{
    public static Dictionary<string, double> SummarizeRollingBalanceGroupsForOverride(List<RollingBalanceGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<LenientAccrualOverride> DeriveLenientAccrualOverridesFromGroup(List<RollingBalanceGroup> items)
    {
        var outValue = new List<LenientAccrualOverride>();
        foreach (var item in items)
        {
            var derived = LenientAccrualOverride.NewLenientAccrualOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLenientAccrualOverride(LenientAccrualOverride item)
    {
        return LenientBalanceCheckpointPolicy.ClassifyLenientBalanceCheckpoint(item.ProjectedLoad());
    }
}
