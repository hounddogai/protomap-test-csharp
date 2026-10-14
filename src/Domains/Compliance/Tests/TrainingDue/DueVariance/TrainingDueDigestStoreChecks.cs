namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueDigestStoreChecks
{
    public static void MemoryTrainingDueDigestStoreFindsWhatItSaved()
    {
        var store = TrainingDueDigestStore.NewMemoryTrainingDueDigestStore();
        var item = TrainingDueDigest.NewTrainingDueDigest("code-281", "label-154");
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
        MemoryTrainingDueDigestStoreFindsWhatItSaved();
    }
}
