namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityAllocationStoreChecks
{
    public static void MemoryMobilityAllocationStoreFindsWhatItSaved()
    {
        var store = MobilityAllocationStore.NewMemoryMobilityAllocationStore();
        var item = MobilityAllocation.NewMobilityAllocation("code-482", "name-887");
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
        MemoryMobilityAllocationStoreFindsWhatItSaved();
    }
}
