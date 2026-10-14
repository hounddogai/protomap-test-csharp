namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantWindowStoreChecks
{
    public static void MemoryEquityGrantWindowStoreFindsWhatItSaved()
    {
        var store = EquityGrantWindowStore.NewMemoryEquityGrantWindowStore();
        var item = EquityGrantWindow.NewEquityGrantWindow("code-734", "label-585");
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
        MemoryEquityGrantWindowStoreFindsWhatItSaved();
    }
}
