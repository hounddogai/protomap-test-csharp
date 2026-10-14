namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueTier;

using Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>Checks of due tier code, run by the project's runner.</summary>
public static class BatchedTrainingDueNoticeStoreChecks
{
    public static void MemoryBatchedTrainingDueNoticeStoreFindsWhatItSaved()
    {
        var store = BatchedTrainingDueNoticeStore.NewMemoryBatchedTrainingDueNoticeStore();
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("code-785", "name-139");
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
        MemoryBatchedTrainingDueNoticeStoreFindsWhatItSaved();
    }
}
