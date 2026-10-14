namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class LegacyMobilityOverrideStoreChecks
{
    public static void MemoryLegacyMobilityOverrideStoreFindsWhatItSaved()
    {
        var store = LegacyMobilityOverrideStore.NewMemoryLegacyMobilityOverrideStore();
        var item = LegacyMobilityOverride.NewLegacyMobilityOverride("code-767", "label-826");
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
        MemoryLegacyMobilityOverrideStoreFindsWhatItSaved();
    }
}
