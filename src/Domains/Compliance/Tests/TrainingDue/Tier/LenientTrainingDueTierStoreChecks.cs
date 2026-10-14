namespace Acme.Workforce.Compliance.Tests.TrainingDue.Tier;

using Acme.Workforce.Compliance.TrainingDue.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientTrainingDueTierStoreChecks
{
    public static void MemoryLenientTrainingDueTierStoreFindsWhatItSaved()
    {
        var store = LenientTrainingDueTierStore.NewMemoryLenientTrainingDueTierStore();
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("code-213", "title-486");
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
        MemoryLenientTrainingDueTierStoreFindsWhatItSaved();
    }
}
