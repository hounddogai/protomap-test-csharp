namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunAllocationStoreChecks
{
    public static void MemoryPayRunAllocationStoreFindsWhatItSaved()
    {
        var store = PayRunAllocationStore.NewMemoryPayRunAllocationStore();
        var item = PayRunAllocation.NewPayRunAllocation("code-815", "label-154");
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
        MemoryPayRunAllocationStoreFindsWhatItSaved();
    }
}
