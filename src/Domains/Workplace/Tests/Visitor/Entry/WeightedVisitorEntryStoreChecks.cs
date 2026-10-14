namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedVisitorEntryStoreChecks
{
    public static void MemoryWeightedVisitorEntryStoreFindsWhatItSaved()
    {
        var store = WeightedVisitorEntryStore.NewMemoryWeightedVisitorEntryStore();
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("code-953", "name-789");
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
        MemoryWeightedVisitorEntryStoreFindsWhatItSaved();
    }
}
