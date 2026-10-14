namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class MobilityGroupStoreChecks
{
    public static void MemoryMobilityGroupStoreFindsWhatItSaved()
    {
        var store = MobilityGroupStore.NewMemoryMobilityGroupStore();
        var item = MobilityGroup.NewMobilityGroup("code-128", "name-942");
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
        MemoryMobilityGroupStoreFindsWhatItSaved();
    }
}
