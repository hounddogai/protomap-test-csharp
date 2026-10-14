namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionProposalStoreChecks
{
    public static void MemoryDeductionProposalStoreFindsWhatItSaved()
    {
        var store = DeductionProposalStore.NewMemoryDeductionProposalStore();
        var item = DeductionProposal.NewDeductionProposal("code-842", "name-322");
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
        MemoryDeductionProposalStoreFindsWhatItSaved();
    }
}
