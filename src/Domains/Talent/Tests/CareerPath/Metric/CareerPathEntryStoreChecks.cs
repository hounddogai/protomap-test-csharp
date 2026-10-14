namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathEntryStoreChecks
{
    public static void MemoryCareerPathEntryStoreFindsWhatItSaved()
    {
        var store = CareerPathEntryStore.NewMemoryCareerPathEntryStore();
        var item = CareerPathEntry.NewCareerPathEntry("code-798", "name-60");
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
        MemoryCareerPathEntryStoreFindsWhatItSaved();
    }
}
