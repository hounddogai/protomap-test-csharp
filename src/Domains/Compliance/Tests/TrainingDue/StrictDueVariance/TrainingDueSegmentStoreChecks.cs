namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueSegmentStoreChecks
{
    public static void MemoryTrainingDueSegmentStoreFindsWhatItSaved()
    {
        var store = TrainingDueSegmentStore.NewMemoryTrainingDueSegmentStore();
        var item = TrainingDueSegment.NewTrainingDueSegment("code-304", "name-297");
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
        MemoryTrainingDueSegmentStoreFindsWhatItSaved();
    }
}
