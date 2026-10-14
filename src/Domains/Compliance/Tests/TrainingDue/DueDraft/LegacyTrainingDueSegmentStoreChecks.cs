namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>Checks of due draft code, run by the project's runner.</summary>
public static class LegacyTrainingDueSegmentStoreChecks
{
    public static void MemoryLegacyTrainingDueSegmentStoreFindsWhatItSaved()
    {
        var store = LegacyTrainingDueSegmentStore.NewMemoryLegacyTrainingDueSegmentStore();
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("code-822", "label-968");
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
        MemoryLegacyTrainingDueSegmentStoreFindsWhatItSaved();
    }
}
