namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionProposalStoreChecks
{
    public static void MemoryRecognitionProposalStoreFindsWhatItSaved()
    {
        var store = RecognitionProposalStore.NewMemoryRecognitionProposalStore();
        var item = RecognitionProposal.NewRecognitionProposal("code-42", "name-778");
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
        MemoryRecognitionProposalStoreFindsWhatItSaved();
    }
}
