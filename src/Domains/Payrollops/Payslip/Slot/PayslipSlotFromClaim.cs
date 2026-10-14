namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Functions of payslip slot from claim.</summary>
public static class PayslipSlotFromClaim
{
    public static Dictionary<string, double> SummarizeTaxFormClaimsForSlot(List<TaxFormClaim> items)
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

    public static List<PayslipSlot> DerivePayslipSlotsFromClaim(List<TaxFormClaim> items)
    {
        var outValue = new List<PayslipSlot>();
        foreach (var item in items)
        {
            var derived = PayslipSlot.NewPayslipSlot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePayslipSlot(PayslipSlot item)
    {
        return TaxFormRulePolicy.TaxFormRuleBucket(item.EffectiveLoad());
    }
}
