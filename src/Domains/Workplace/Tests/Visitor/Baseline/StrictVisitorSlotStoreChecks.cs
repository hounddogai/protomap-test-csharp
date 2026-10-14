namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictVisitorSlotStoreChecks
{
    public static void MemoryStrictVisitorSlotStoreFindsWhatItSaved()
    {
        var store = StrictVisitorSlotStore.NewMemoryStrictVisitorSlotStore();
        var item = StrictVisitorSlot.NewStrictVisitorSlot("code-425", "name-57");
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
        MemoryStrictVisitorSlotStoreFindsWhatItSaved();
    }
}
