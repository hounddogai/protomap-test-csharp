namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantRollupStoreChecks
{
    public static void MemoryEquityGrantRollupStoreFindsWhatItSaved()
    {
        var store = EquityGrantRollupStore.NewMemoryEquityGrantRollupStore();
        var item = EquityGrantRollup.NewEquityGrantRollup("code-624", "name-214");
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
        MemoryEquityGrantRollupStoreFindsWhatItSaved();
    }
}
