namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredTrainingDueAuditStoreChecks
{
    public static void MemoryTieredTrainingDueAuditStoreFindsWhatItSaved()
    {
        var store = TieredTrainingDueAuditStore.NewMemoryTieredTrainingDueAuditStore();
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("code-557", "name-559");
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
        MemoryTieredTrainingDueAuditStoreFindsWhatItSaved();
    }
}
