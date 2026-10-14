namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackCostModelRollupStoreChecks
{
    public static void MemoryFallbackCostModelRollupStoreFindsWhatItSaved()
    {
        var store = FallbackCostModelRollupStore.NewMemoryFallbackCostModelRollupStore();
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("code-649", "name-914");
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
        MemoryFallbackCostModelRollupStoreFindsWhatItSaved();
    }
}
