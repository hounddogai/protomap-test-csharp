namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class StrictCareerPathOverrideStoreChecks
{
    public static void MemoryStrictCareerPathOverrideStoreFindsWhatItSaved()
    {
        var store = StrictCareerPathOverrideStore.NewMemoryStrictCareerPathOverrideStore();
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("code-51", "title-114");
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
        MemoryStrictCareerPathOverrideStoreFindsWhatItSaved();
    }
}
