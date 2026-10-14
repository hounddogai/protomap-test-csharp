namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalPayRunGroupStoreChecks
{
    public static void MemoryRegionalPayRunGroupStoreFindsWhatItSaved()
    {
        var store = RegionalPayRunGroupStore.NewMemoryRegionalPayRunGroupStore();
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("code-923", "name-878");
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
        MemoryRegionalPayRunGroupStoreFindsWhatItSaved();
    }
}
