namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultRateCycleStoreChecks
{
    public static void MemoryDefaultRateCycleStoreFindsWhatItSaved()
    {
        var store = DefaultRateCycleStore.NewMemoryDefaultRateCycleStore();
        var item = DefaultRateCycle.NewDefaultRateCycle("code-932", "title-765");
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
        MemoryDefaultRateCycleStoreFindsWhatItSaved();
    }
}
