namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedRetroPayProposalStoreChecks
{
    public static void MemoryCappedRetroPayProposalStoreFindsWhatItSaved()
    {
        var store = CappedRetroPayProposalStore.NewMemoryCappedRetroPayProposalStore();
        var item = CappedRetroPayProposal.NewCappedRetroPayProposal("code-560", "title-351");
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
        MemoryCappedRetroPayProposalStoreFindsWhatItSaved();
    }
}
