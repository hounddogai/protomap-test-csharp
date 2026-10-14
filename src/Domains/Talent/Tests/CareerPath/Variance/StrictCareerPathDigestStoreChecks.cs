namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictCareerPathDigestStoreChecks
{
    public static void MemoryStrictCareerPathDigestStoreFindsWhatItSaved()
    {
        var store = StrictCareerPathDigestStore.NewMemoryStrictCareerPathDigestStore();
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("code-126", "name-233");
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
        MemoryStrictCareerPathDigestStoreFindsWhatItSaved();
    }
}
