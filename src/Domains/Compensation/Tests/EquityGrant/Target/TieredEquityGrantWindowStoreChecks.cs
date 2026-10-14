namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredEquityGrantWindowStoreChecks
{
    public static void MemoryTieredEquityGrantWindowStoreFindsWhatItSaved()
    {
        var store = TieredEquityGrantWindowStore.NewMemoryTieredEquityGrantWindowStore();
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("code-401", "name-199");
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
        MemoryTieredEquityGrantWindowStoreFindsWhatItSaved();
    }
}
