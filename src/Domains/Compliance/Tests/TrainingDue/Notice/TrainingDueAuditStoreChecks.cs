namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueAuditStoreChecks
{
    public static void MemoryTrainingDueAuditStoreFindsWhatItSaved()
    {
        var store = TrainingDueAuditStore.NewMemoryTrainingDueAuditStore();
        var item = TrainingDueAudit.NewTrainingDueAudit("code-898", "title-406");
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
        MemoryTrainingDueAuditStoreFindsWhatItSaved();
    }
}
