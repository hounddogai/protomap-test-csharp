namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class RollingPayRunCycleStoreChecks
{
    public static void MemoryRollingPayRunCycleStoreFindsWhatItSaved()
    {
        var store = RollingPayRunCycleStore.NewMemoryRollingPayRunCycleStore();
        var item = RollingPayRunCycle.NewRollingPayRunCycle("code-57", "label-752");
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
        MemoryRollingPayRunCycleStoreFindsWhatItSaved();
    }
}
