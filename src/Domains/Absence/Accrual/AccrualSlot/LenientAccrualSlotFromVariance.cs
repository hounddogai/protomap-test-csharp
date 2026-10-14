namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Functions of lenient accrual slot from variance.</summary>
public static class LenientAccrualSlotFromVariance
{
    public static Dictionary<string, double> SummarizePolicyVariancesForSlot(List<PolicyVariance> items)
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

    public static List<LenientAccrualSlot> DeriveLenientAccrualSlotsFromVariance(List<PolicyVariance> items)
    {
        var outValue = new List<LenientAccrualSlot>();
        foreach (var item in items)
        {
            var derived = LenientAccrualSlot.NewLenientAccrualSlot(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveLenientAccrualSlot(LenientAccrualSlot item)
    {
        return StrictPolicyPreferencePolicy.StrictPolicyPreferenceWeight(8, true);
    }
}
