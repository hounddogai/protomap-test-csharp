namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictMobilityOverrideStoreChecks
{
    public static void MemoryStrictMobilityOverrideStoreFindsWhatItSaved()
    {
        var store = StrictMobilityOverrideStore.NewMemoryStrictMobilityOverrideStore();
        var item = StrictMobilityOverride.NewStrictMobilityOverride("code-244", "title-456");
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
        MemoryStrictMobilityOverrideStoreFindsWhatItSaved();
    }
}
