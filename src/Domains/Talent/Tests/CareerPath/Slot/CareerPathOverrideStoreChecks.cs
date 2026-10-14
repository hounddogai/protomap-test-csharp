namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathOverrideStoreChecks
{
    public static void MemoryCareerPathOverrideStoreFindsWhatItSaved()
    {
        var store = CareerPathOverrideStore.NewMemoryCareerPathOverrideStore();
        var item = CareerPathOverride.NewCareerPathOverride("code-221", "name-787");
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
        MemoryCareerPathOverrideStoreFindsWhatItSaved();
    }
}
