namespace Acme.Workforce.Workplace.Tests.Desk.Rule;

using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingDeskRollupStoreChecks
{
    public static void MemoryPendingDeskRollupStoreFindsWhatItSaved()
    {
        var store = PendingDeskRollupStore.NewMemoryPendingDeskRollupStore();
        var item = PendingDeskRollup.NewPendingDeskRollup("code-767", "name-618");
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
        MemoryPendingDeskRollupStoreFindsWhatItSaved();
    }
}
