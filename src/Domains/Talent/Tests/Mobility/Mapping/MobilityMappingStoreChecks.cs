namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MobilityMappingStoreChecks
{
    public static void MemoryMobilityMappingStoreFindsWhatItSaved()
    {
        var store = MobilityMappingStore.NewMemoryMobilityMappingStore();
        var item = MobilityMapping.NewMobilityMapping("code-897", "title-357");
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
        MemoryMobilityMappingStoreFindsWhatItSaved();
    }
}
