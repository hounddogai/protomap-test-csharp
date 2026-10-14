namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Proposal;

/// <summary>Functions of batched payslip proposal from proposal.</summary>
public static class BatchedPayslipProposalFromProposal
{
    public static Dictionary<string, double> SummarizeTaxFormProposalsForProposal(List<TaxFormProposal> items)
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

    public static List<BatchedPayslipProposal> DeriveBatchedPayslipProposalsFromProposal(List<TaxFormProposal> items)
    {
        var outValue = new List<BatchedPayslipProposal>();
        foreach (var item in items)
        {
            var derived = BatchedPayslipProposal.NewBatchedPayslipProposal(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeBatchedPayslipProposal(BatchedPayslipProposal item)
    {
        return TaxFormNoticePolicy.TaxFormNoticeBucket(item.EffectiveTotal());
    }
}
