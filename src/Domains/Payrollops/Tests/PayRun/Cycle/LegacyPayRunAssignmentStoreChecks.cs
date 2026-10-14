namespace Acme.Workforce.Payrollops.Tests.PayRun.Cycle;

using Acme.Workforce.Payrollops.PayRun.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LegacyPayRunAssignmentStoreChecks
{
    public static void MemoryLegacyPayRunAssignmentStoreFindsWhatItSaved()
    {
        var store = LegacyPayRunAssignmentStore.NewMemoryLegacyPayRunAssignmentStore();
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("code-250", "name-109");
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
        MemoryLegacyPayRunAssignmentStoreFindsWhatItSaved();
    }
}
