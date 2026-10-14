namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityClaimStoreChecks
{
    public static void MemoryMobilityClaimStoreFindsWhatItSaved()
    {
        var store = MobilityClaimStore.NewMemoryMobilityClaimStore();
        var item = MobilityClaim.NewMobilityClaim("code-990", "name-964");
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
        MemoryMobilityClaimStoreFindsWhatItSaved();
    }
}
