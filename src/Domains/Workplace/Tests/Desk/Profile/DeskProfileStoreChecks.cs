namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskProfileStoreChecks
{
    public static void MemoryDeskProfileStoreFindsWhatItSaved()
    {
        var store = DeskProfileStore.NewMemoryDeskProfileStore();
        var item = DeskProfile.NewDeskProfile("code-620", "name-930");
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
        MemoryDeskProfileStoreFindsWhatItSaved();
    }
}
