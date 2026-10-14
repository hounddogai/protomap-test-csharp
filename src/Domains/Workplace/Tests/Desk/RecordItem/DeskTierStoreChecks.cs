namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskTierStoreChecks
{
    public static void MemoryDeskTierStoreFindsWhatItSaved()
    {
        var store = DeskTierStore.NewMemoryDeskTierStore();
        var item = DeskTier.NewDeskTier("code-790", "name-884");
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
        MemoryDeskTierStoreFindsWhatItSaved();
    }
}
