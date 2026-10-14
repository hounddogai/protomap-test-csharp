namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorSlotStoreChecks
{
    public static void MemoryVisitorSlotStoreFindsWhatItSaved()
    {
        var store = VisitorSlotStore.NewMemoryVisitorSlotStore();
        var item = VisitorSlot.NewVisitorSlot("code-465", "label-865");
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
        MemoryVisitorSlotStoreFindsWhatItSaved();
    }
}
