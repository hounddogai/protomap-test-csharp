namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyPoolVarianceStoreChecks
{
    public static void MemoryLegacyPoolVarianceStoreFindsWhatItSaved()
    {
        var store = LegacyPoolVarianceStore.NewMemoryLegacyPoolVarianceStore();
        var item = LegacyPoolVariance.NewLegacyPoolVariance("code-846", "title-32");
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
        MemoryLegacyPoolVarianceStoreFindsWhatItSaved();
    }
}
