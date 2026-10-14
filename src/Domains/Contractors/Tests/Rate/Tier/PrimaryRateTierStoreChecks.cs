namespace Acme.Workforce.Contractors.Tests.Rate.Tier;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PrimaryRateTierStoreChecks
{
    public static void MemoryPrimaryRateTierStoreFindsWhatItSaved()
    {
        var store = PrimaryRateTierStore.NewMemoryPrimaryRateTierStore();
        var item = PrimaryRateTier.NewPrimaryRateTier("code-353", "title-235");
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
        MemoryPrimaryRateTierStoreFindsWhatItSaved();
    }
}
