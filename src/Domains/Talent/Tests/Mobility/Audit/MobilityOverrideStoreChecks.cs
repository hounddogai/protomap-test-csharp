namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityOverrideStoreChecks
{
    public static void MemoryMobilityOverrideStoreFindsWhatItSaved()
    {
        var store = MobilityOverrideStore.NewMemoryMobilityOverrideStore();
        var item = MobilityOverride.NewMobilityOverride("code-335", "name-773");
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
        MemoryMobilityOverrideStoreFindsWhatItSaved();
    }
}
