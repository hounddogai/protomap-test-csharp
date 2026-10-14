namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Functions of legacy deduction preference from allocation.</summary>
public static class LegacyDeductionPreferenceFromAllocation
{
    public static Dictionary<string, double> SummarizeDefaultRetroPayAllocationsForPreference(List<DefaultRetroPayAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<LegacyDeductionPreference> DeriveLegacyDeductionPreferencesFromAllocation(List<DefaultRetroPayAllocation> items)
    {
        var outValue = new List<LegacyDeductionPreference>();
        foreach (var item in items)
        {
            var derived = LegacyDeductionPreference.NewLegacyDeductionPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLegacyDeductionPreference(LegacyDeductionPreference item)
    {
        return LegacyRetroPayPlanPolicy.ClassifyLegacyRetroPayPlan(item.WeightedVolume());
    }
}
