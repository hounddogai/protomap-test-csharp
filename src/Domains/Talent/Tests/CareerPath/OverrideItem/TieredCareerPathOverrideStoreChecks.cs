namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredCareerPathOverrideStoreChecks
{
    public static void MemoryTieredCareerPathOverrideStoreFindsWhatItSaved()
    {
        var store = TieredCareerPathOverrideStore.NewMemoryTieredCareerPathOverrideStore();
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("code-643", "label-595");
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
        MemoryTieredCareerPathOverrideStoreFindsWhatItSaved();
    }
}
