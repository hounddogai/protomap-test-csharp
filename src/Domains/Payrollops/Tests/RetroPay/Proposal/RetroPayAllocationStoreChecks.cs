namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayAllocationStoreChecks
{
    public static void MemoryRetroPayAllocationStoreFindsWhatItSaved()
    {
        var store = RetroPayAllocationStore.NewMemoryRetroPayAllocationStore();
        var item = RetroPayAllocation.NewRetroPayAllocation("code-399", "label-484");
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
        MemoryRetroPayAllocationStoreFindsWhatItSaved();
    }
}
