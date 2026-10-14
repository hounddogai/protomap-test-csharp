namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class RegionalPayRunTemplateStoreChecks
{
    public static void MemoryRegionalPayRunTemplateStoreFindsWhatItSaved()
    {
        var store = RegionalPayRunTemplateStore.NewMemoryRegionalPayRunTemplateStore();
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("code-179", "label-737");
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
        MemoryRegionalPayRunTemplateStoreFindsWhatItSaved();
    }
}
