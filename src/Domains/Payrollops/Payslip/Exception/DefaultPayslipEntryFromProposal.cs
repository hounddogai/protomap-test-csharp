namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Assignment;

/// <summary>Functions of default payslip entry from proposal.</summary>
public static class DefaultPayslipEntryFromProposal
{
    public static Dictionary<string, double> SummarizeBatchedPayslipProposalsForEntry(List<BatchedPayslipProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<DefaultPayslipEntry> DeriveDefaultPayslipEntriesFromProposal(List<BatchedPayslipProposal> items)
    {
        var outValue = new List<DefaultPayslipEntry>();
        foreach (var item in items)
        {
            var derived = DefaultPayslipEntry.NewDefaultPayslipEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateDefaultPayslipEntry(DefaultPayslipEntry item)
    {
        return PayslipAssignmentPolicy.NormalizePayslipAssignmentCode(item.Code);
    }
}
