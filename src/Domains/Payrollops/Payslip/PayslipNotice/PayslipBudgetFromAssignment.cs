namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Functions of payslip budget from assignment.</summary>
public static class PayslipBudgetFromAssignment
{
    public static Dictionary<string, double> SummarizeLegacyTaxFormAssignmentsForBudget(List<LegacyTaxFormAssignment> items)
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

    public static List<PayslipBudget> DerivePayslipBudgetsFromAssignment(List<LegacyTaxFormAssignment> items)
    {
        var outValue = new List<PayslipBudget>();
        foreach (var item in items)
        {
            var derived = PayslipBudget.NewPayslipBudget(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPayslipBudget(PayslipBudget item)
    {
        return TaxFormReviewPolicy.TaxFormReviewBucket(item.AdjustedVolume());
    }
}
