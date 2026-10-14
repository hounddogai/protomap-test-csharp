namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkProposalStoreChecks
{
    public static void MemoryStatementOfWorkProposalStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkProposalStore.NewMemoryStatementOfWorkProposalStore();
        var item = StatementOfWorkProposal.NewStatementOfWorkProposal("code-991", "title-860");
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
        MemoryStatementOfWorkProposalStoreFindsWhatItSaved();
    }
}
