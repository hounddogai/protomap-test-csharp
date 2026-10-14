namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Functions of payslip proposal from audit.</summary>
public static class PayslipProposalFromAudit
{
    public static Dictionary<string, double> SummarizeManualTaxFormAuditsForProposal(List<ManualTaxFormAudit> items)
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

    public static List<PayslipProposal> DerivePayslipProposalsFromAudit(List<ManualTaxFormAudit> items)
    {
        var outValue = new List<PayslipProposal>();
        foreach (var item in items)
        {
            var derived = PayslipProposal.NewPayslipProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePayslipProposal(PayslipProposal item)
    {
        return ManualTaxFormAuditPolicy.ManualTaxFormAuditBucket(item.NetLoad());
    }
}
