namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Functions of tax form override from slot.</summary>
public static class TaxFormOverrideFromSlot
{
    public static Dictionary<string, double> SummarizeSeasonalPayslipSlotsForOverride(List<SeasonalPayslipSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<TaxFormOverride> DeriveTaxFormOverridesFromSlot(List<SeasonalPayslipSlot> items)
    {
        var outValue = new List<TaxFormOverride>();
        foreach (var item in items)
        {
            var derived = TaxFormOverride.NewTaxFormOverride(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTaxFormOverride(TaxFormOverride item)
    {
        return SeasonalPayslipSlotPolicy.NormalizeSeasonalPayslipSlotCode(item.Code);
    }
}
