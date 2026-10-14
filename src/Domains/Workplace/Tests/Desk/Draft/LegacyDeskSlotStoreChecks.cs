namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyDeskSlotStoreChecks
{
    public static void MemoryLegacyDeskSlotStoreFindsWhatItSaved()
    {
        var store = LegacyDeskSlotStore.NewMemoryLegacyDeskSlotStore();
        var item = LegacyDeskSlot.NewLegacyDeskSlot("code-321", "title-487");
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
        MemoryLegacyDeskSlotStoreFindsWhatItSaved();
    }
}
