namespace Acme.Workforce.Payrollops.Tests.Payslip.Assignment;

using Acme.Workforce.Payrollops.Payslip.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BatchedPayslipProposalStoreChecks
{
    public static void MemoryBatchedPayslipProposalStoreFindsWhatItSaved()
    {
        var store = BatchedPayslipProposalStore.NewMemoryBatchedPayslipProposalStore();
        var item = BatchedPayslipProposal.NewBatchedPayslipProposal("code-66", "name-600");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedPayslipProposalStoreFindsWhatItSaved();
    }
}
