namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class RollingRecognitionProposalStoreChecks
{
    public static void MemoryRollingRecognitionProposalStoreFindsWhatItSaved()
    {
        var store = RollingRecognitionProposalStore.NewMemoryRollingRecognitionProposalStore();
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("code-295", "label-71");
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
        MemoryRollingRecognitionProposalStoreFindsWhatItSaved();
    }
}
