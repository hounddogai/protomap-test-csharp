namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalPayslipProposalStoreChecks
{
    public static void MemoryGlobalPayslipProposalStoreFindsWhatItSaved()
    {
        var store = GlobalPayslipProposalStore.NewMemoryGlobalPayslipProposalStore();
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("code-361", "title-296");
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
        MemoryGlobalPayslipProposalStoreFindsWhatItSaved();
    }
}
