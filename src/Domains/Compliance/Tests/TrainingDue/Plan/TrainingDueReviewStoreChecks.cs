namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueReviewStoreChecks
{
    public static void MemoryTrainingDueReviewStoreFindsWhatItSaved()
    {
        var store = TrainingDueReviewStore.NewMemoryTrainingDueReviewStore();
        var item = TrainingDueReview.NewTrainingDueReview("code-434", "label-533");
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
        MemoryTrainingDueReviewStoreFindsWhatItSaved();
    }
}
