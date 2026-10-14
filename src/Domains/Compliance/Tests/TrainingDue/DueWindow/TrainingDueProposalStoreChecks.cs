namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class TrainingDueProposalStoreChecks
{
    public static void MemoryTrainingDueProposalStoreFindsWhatItSaved()
    {
        var store = TrainingDueProposalStore.NewMemoryTrainingDueProposalStore();
        var item = TrainingDueProposal.NewTrainingDueProposal("code-483", "name-253");
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
        MemoryTrainingDueProposalStoreFindsWhatItSaved();
    }
}
