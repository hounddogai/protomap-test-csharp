namespace Acme.Workforce.Workplace.Tests.Visitor.Target;

using Acme.Workforce.Workplace.Visitor.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientVisitorEntryStoreChecks
{
    public static void MemoryLenientVisitorEntryStoreFindsWhatItSaved()
    {
        var store = LenientVisitorEntryStore.NewMemoryLenientVisitorEntryStore();
        var item = LenientVisitorEntry.NewLenientVisitorEntry("code-699", "name-696");
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
        MemoryLenientVisitorEntryStoreFindsWhatItSaved();
    }
}
