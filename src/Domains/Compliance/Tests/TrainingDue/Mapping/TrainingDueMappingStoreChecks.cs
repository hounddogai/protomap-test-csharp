namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueMappingStoreChecks
{
    public static void MemoryTrainingDueMappingStoreFindsWhatItSaved()
    {
        var store = TrainingDueMappingStore.NewMemoryTrainingDueMappingStore();
        var item = TrainingDueMapping.NewTrainingDueMapping("code-687", "name-57");
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
        MemoryTrainingDueMappingStoreFindsWhatItSaved();
    }
}
