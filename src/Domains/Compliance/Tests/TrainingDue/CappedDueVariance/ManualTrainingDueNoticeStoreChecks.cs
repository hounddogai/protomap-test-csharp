namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class ManualTrainingDueNoticeStoreChecks
{
    public static void MemoryManualTrainingDueNoticeStoreFindsWhatItSaved()
    {
        var store = ManualTrainingDueNoticeStore.NewMemoryManualTrainingDueNoticeStore();
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("code-776", "name-901");
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
        MemoryManualTrainingDueNoticeStoreFindsWhatItSaved();
    }
}
