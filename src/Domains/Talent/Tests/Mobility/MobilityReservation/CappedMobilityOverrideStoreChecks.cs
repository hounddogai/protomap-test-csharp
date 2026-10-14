namespace Acme.Workforce.Talent.Tests.Mobility.MobilityReservation;

using Acme.Workforce.Talent.Mobility.MobilityReservation;

/// <summary>Checks of mobility reservation code, run by the project's runner.</summary>
public static class CappedMobilityOverrideStoreChecks
{
    public static void MemoryCappedMobilityOverrideStoreFindsWhatItSaved()
    {
        var store = CappedMobilityOverrideStore.NewMemoryCappedMobilityOverrideStore();
        var item = CappedMobilityOverride.NewCappedMobilityOverride("code-592", "title-148");
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
        MemoryCappedMobilityOverrideStoreFindsWhatItSaved();
    }
}
