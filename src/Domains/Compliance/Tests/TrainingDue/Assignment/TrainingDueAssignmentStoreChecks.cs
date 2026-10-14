namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueAssignmentStoreChecks
{
    public static void MemoryTrainingDueAssignmentStoreFindsWhatItSaved()
    {
        var store = TrainingDueAssignmentStore.NewMemoryTrainingDueAssignmentStore();
        var item = TrainingDueAssignment.NewTrainingDueAssignment("code-743", "title-790");
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
        MemoryTrainingDueAssignmentStoreFindsWhatItSaved();
    }
}
