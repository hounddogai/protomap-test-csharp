namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StrictSpanProposalStoreChecks
{
    public static void MemoryStrictSpanProposalStoreFindsWhatItSaved()
    {
        var store = StrictSpanProposalStore.NewMemoryStrictSpanProposalStore();
        var item = StrictSpanProposal.NewStrictSpanProposal("code-465", "title-392");
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
        MemoryStrictSpanProposalStoreFindsWhatItSaved();
    }
}
