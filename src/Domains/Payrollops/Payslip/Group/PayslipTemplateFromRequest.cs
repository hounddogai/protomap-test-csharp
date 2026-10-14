namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Functions of payslip template from request.</summary>
public static class PayslipTemplateFromRequest
{
    public static Dictionary<string, double> SummarizePayslipRequestsForTemplate(List<PayslipRequest> items)
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

    public static List<PayslipTemplate> DerivePayslipTemplatesFromRequest(List<PayslipRequest> items)
    {
        var outValue = new List<PayslipTemplate>();
        foreach (var item in items)
        {
            var derived = PayslipTemplate.NewPayslipTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePayslipTemplate(PayslipTemplate item)
    {
        return PayslipRequestPolicy.ClassifyPayslipRequest(item.ProjectedLoad());
    }
}
