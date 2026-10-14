namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PendingExceptionProposalStoreChecks
{
    public static void MemoryPendingExceptionProposalStoreFindsWhatItSaved()
    {
        var store = PendingExceptionProposalStore.NewMemoryPendingExceptionProposalStore();
        var item = PendingExceptionProposal.NewPendingExceptionProposal("code-498", "title-702");
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
        MemoryPendingExceptionProposalStoreFindsWhatItSaved();
    }
}
