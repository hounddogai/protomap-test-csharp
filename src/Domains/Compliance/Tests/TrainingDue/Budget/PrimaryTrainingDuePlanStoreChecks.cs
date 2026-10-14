namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryTrainingDuePlanStoreChecks
{
    public static void MemoryPrimaryTrainingDuePlanStoreFindsWhatItSaved()
    {
        var store = PrimaryTrainingDuePlanStore.NewMemoryPrimaryTrainingDuePlanStore();
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("code-861", "title-972");
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
        MemoryPrimaryTrainingDuePlanStoreFindsWhatItSaved();
    }
}
