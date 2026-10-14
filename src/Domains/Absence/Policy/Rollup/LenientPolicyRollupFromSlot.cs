namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Functions of lenient policy rollup from slot.</summary>
public static class LenientPolicyRollupFromSlot
{
    public static Dictionary<string, double> SummarizeReturnPlanSlotsForRollup(List<ReturnPlanSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LenientPolicyRollup> DeriveLenientPolicyRollupsFromSlot(List<ReturnPlanSlot> items)
    {
        var outValue = new List<LenientPolicyRollup>();
        foreach (var item in items)
        {
            var derived = LenientPolicyRollup.NewLenientPolicyRollup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLenientPolicyRollup(LenientPolicyRollup item)
    {
        return ReturnPlanSlotPolicy.ClassifyReturnPlanSlot(item.NetVolume());
    }
}
