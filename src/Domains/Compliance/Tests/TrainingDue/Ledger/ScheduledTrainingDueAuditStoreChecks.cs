namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ScheduledTrainingDueAuditStoreChecks
{
    public static void MemoryScheduledTrainingDueAuditStoreFindsWhatItSaved()
    {
        var store = ScheduledTrainingDueAuditStore.NewMemoryScheduledTrainingDueAuditStore();
        var item = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit("code-827", "name-935");
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
        MemoryScheduledTrainingDueAuditStoreFindsWhatItSaved();
    }
}
