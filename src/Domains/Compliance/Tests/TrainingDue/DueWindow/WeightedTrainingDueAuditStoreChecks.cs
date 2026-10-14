namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class WeightedTrainingDueAuditStoreChecks
{
    public static void MemoryWeightedTrainingDueAuditStoreFindsWhatItSaved()
    {
        var store = WeightedTrainingDueAuditStore.NewMemoryWeightedTrainingDueAuditStore();
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("code-46", "title-945");
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
        MemoryWeightedTrainingDueAuditStoreFindsWhatItSaved();
    }
}
