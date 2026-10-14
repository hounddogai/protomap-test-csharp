namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ManualEquityGrantWindowStoreChecks
{
    public static void MemoryManualEquityGrantWindowStoreFindsWhatItSaved()
    {
        var store = ManualEquityGrantWindowStore.NewMemoryManualEquityGrantWindowStore();
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("code-753", "title-128");
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
        MemoryManualEquityGrantWindowStoreFindsWhatItSaved();
    }
}
