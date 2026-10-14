namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledPayRunRequestStoreChecks
{
    public static void MemoryScheduledPayRunRequestStoreFindsWhatItSaved()
    {
        var store = ScheduledPayRunRequestStore.NewMemoryScheduledPayRunRequestStore();
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("code-277", "name-155");
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
        MemoryScheduledPayRunRequestStoreFindsWhatItSaved();
    }
}
