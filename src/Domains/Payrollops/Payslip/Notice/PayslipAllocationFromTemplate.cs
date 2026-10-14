namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Functions of payslip allocation from template.</summary>
public static class PayslipAllocationFromTemplate
{
    public static Dictionary<string, double> SummarizeTaxFormTemplatesForAllocation(List<TaxFormTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PayslipAllocation> DerivePayslipAllocationsFromTemplate(List<TaxFormTemplate> items)
    {
        var outValue = new List<PayslipAllocation>();
        foreach (var item in items)
        {
            var derived = PayslipAllocation.NewPayslipAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePayslipAllocation(PayslipAllocation item)
    {
        return TaxFormVariancePolicy.TaxFormVarianceBucket(item.MeasuredValue());
    }
}
