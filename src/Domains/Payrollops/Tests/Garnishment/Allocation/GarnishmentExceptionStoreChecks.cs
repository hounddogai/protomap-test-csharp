namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentExceptionStoreChecks
{
    public static void MemoryGarnishmentExceptionStoreFindsWhatItSaved()
    {
        var store = GarnishmentExceptionStore.NewMemoryGarnishmentExceptionStore();
        var item = GarnishmentException.NewGarnishmentException("code-291", "title-256");
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
        MemoryGarnishmentExceptionStoreFindsWhatItSaved();
    }
}
