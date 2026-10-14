namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueBaselineStoreChecks
{
    public static void MemoryTrainingDueBaselineStoreFindsWhatItSaved()
    {
        var store = TrainingDueBaselineStore.NewMemoryTrainingDueBaselineStore();
        var item = TrainingDueBaseline.NewTrainingDueBaseline("code-558", "label-587");
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
        MemoryTrainingDueBaselineStoreFindsWhatItSaved();
    }
}
