namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Functions of deduction template from notice.</summary>
public static class DeductionTemplateFromNotice
{
    public static Dictionary<string, double> SummarizeGlobalPayslipNoticesForTemplate(List<GlobalPayslipNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<DeductionTemplate> DeriveDeductionTemplatesFromNotice(List<GlobalPayslipNotice> items)
    {
        var outValue = new List<DeductionTemplate>();
        foreach (var item in items)
        {
            var derived = DeductionTemplate.NewDeductionTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyDeductionTemplateExtra(DeductionTemplate item)
    {
        return GlobalPayslipNoticePolicy.NormalizeGlobalPayslipNoticeCode(item.Code);
    }
}
