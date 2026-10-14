namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProposalStoreChecks
{
    public static void MemoryCappedStatementOfWorkProposalStoreFindsWhatItSaved()
    {
        var store = CappedStatementOfWorkProposalStore.NewMemoryCappedStatementOfWorkProposalStore();
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("code-530", "name-991");
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
        MemoryCappedStatementOfWorkProposalStoreFindsWhatItSaved();
    }
}
