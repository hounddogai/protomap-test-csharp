namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskSlotStoreChecks
{
    public static void MemoryDeskSlotStoreFindsWhatItSaved()
    {
        var store = DeskSlotStore.NewMemoryDeskSlotStore();
        var item = DeskSlot.NewDeskSlot("code-151", "label-66");
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
        MemoryDeskSlotStoreFindsWhatItSaved();
    }
}
