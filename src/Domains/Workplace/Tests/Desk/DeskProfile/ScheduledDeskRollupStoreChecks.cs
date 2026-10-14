namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class ScheduledDeskRollupStoreChecks
{
    public static void MemoryScheduledDeskRollupStoreFindsWhatItSaved()
    {
        var store = ScheduledDeskRollupStore.NewMemoryScheduledDeskRollupStore();
        var item = ScheduledDeskRollup.NewScheduledDeskRollup("code-540", "name-182");
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
        MemoryScheduledDeskRollupStoreFindsWhatItSaved();
    }
}
