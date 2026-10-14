namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Functions of lenient balance batch from window.</summary>
public static class LenientBalanceBatchFromWindow
{
    public static Dictionary<string, double> SummarizePendingReturnPlanWindowsForBatch(List<PendingReturnPlanWindow> items)
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

    public static List<LenientBalanceBatch> DeriveLenientBalanceBatchesFromWindow(List<PendingReturnPlanWindow> items)
    {
        var outValue = new List<LenientBalanceBatch>();
        foreach (var item in items)
        {
            var derived = LenientBalanceBatch.NewLenientBalanceBatch(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignLenientBalanceBatch(LenientBalanceBatch item)
    {
        return PendingReturnPlanWindowPolicy.PendingReturnPlanWindowWeight(5, true);
    }
}
