namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingDeductionEntryStoreChecks
{
    public static void MemoryRollingDeductionEntryStoreFindsWhatItSaved()
    {
        var store = RollingDeductionEntryStore.NewMemoryRollingDeductionEntryStore();
        var item = RollingDeductionEntry.NewRollingDeductionEntry("code-822", "title-31");
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
        MemoryRollingDeductionEntryStoreFindsWhatItSaved();
    }
}
