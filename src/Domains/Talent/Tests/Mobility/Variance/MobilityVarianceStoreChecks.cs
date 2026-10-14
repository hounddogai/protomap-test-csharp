namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class MobilityVarianceStoreChecks
{
    public static void MemoryMobilityVarianceStoreFindsWhatItSaved()
    {
        var store = MobilityVarianceStore.NewMemoryMobilityVarianceStore();
        var item = MobilityVariance.NewMobilityVariance("code-400", "title-247");
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
        MemoryMobilityVarianceStoreFindsWhatItSaved();
    }
}
