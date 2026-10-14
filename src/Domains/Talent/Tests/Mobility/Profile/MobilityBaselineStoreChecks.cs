namespace Acme.Workforce.Talent.Tests.Mobility.Profile;

using Acme.Workforce.Talent.Mobility.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MobilityBaselineStoreChecks
{
    public static void MemoryMobilityBaselineStoreFindsWhatItSaved()
    {
        var store = MobilityBaselineStore.NewMemoryMobilityBaselineStore();
        var item = MobilityBaseline.NewMobilityBaseline("code-16", "title-9");
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
        MemoryMobilityBaselineStoreFindsWhatItSaved();
    }
}
