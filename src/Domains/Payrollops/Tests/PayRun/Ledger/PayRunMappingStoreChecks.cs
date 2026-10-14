namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunMappingStoreChecks
{
    public static void MemoryPayRunMappingStoreFindsWhatItSaved()
    {
        var store = PayRunMappingStore.NewMemoryPayRunMappingStore();
        var item = PayRunMapping.NewPayRunMapping("code-153", "name-757");
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
        MemoryPayRunMappingStoreFindsWhatItSaved();
    }
}
