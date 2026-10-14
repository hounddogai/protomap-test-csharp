namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Functions of payslip batch from template.</summary>
public static class PayslipBatchFromTemplate
{
    public static Dictionary<string, double> SummarizeRetroPayTemplatesForBatch(List<RetroPayTemplate> items)
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

    public static List<PayslipBatch> DerivePayslipBatchesFromTemplate(List<RetroPayTemplate> items)
    {
        var outValue = new List<PayslipBatch>();
        foreach (var item in items)
        {
            var derived = PayslipBatch.NewPayslipBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPayslipBatch(PayslipBatch item)
    {
        return RetroPayTemplatePolicy.NormalizeRetroPayTemplateCode(item.Code);
    }
}
