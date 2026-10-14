namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Functions of tax form override from cycle.</summary>
public static class TaxFormOverrideFromCycle
{
    public static Dictionary<string, double> SummarizePayRunCyclesForOverride(List<PayRunCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TaxFormOverride> DeriveTaxFormOverridesFromCycle(List<PayRunCycle> items)
    {
        var outValue = new List<TaxFormOverride>();
        foreach (var item in items)
        {
            var derived = TaxFormOverride.NewTaxFormOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateTaxFormOverride(TaxFormOverride item)
    {
        return PayRunCyclePolicy.PayRunCycleWeight(6, true);
    }
}
