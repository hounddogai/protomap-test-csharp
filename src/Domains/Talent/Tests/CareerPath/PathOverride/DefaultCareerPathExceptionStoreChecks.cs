namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class DefaultCareerPathExceptionStoreChecks
{
    public static void MemoryDefaultCareerPathExceptionStoreFindsWhatItSaved()
    {
        var store = DefaultCareerPathExceptionStore.NewMemoryDefaultCareerPathExceptionStore();
        var item = DefaultCareerPathException.NewDefaultCareerPathException("code-352", "label-131");
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
        MemoryDefaultCareerPathExceptionStoreFindsWhatItSaved();
    }
}
