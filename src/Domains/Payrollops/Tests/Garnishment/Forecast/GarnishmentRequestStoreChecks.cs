namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRequestStoreChecks
{
    public static void MemoryGarnishmentRequestStoreFindsWhatItSaved()
    {
        var store = GarnishmentRequestStore.NewMemoryGarnishmentRequestStore();
        var item = GarnishmentRequest.NewGarnishmentRequest("code-309", "title-295");
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
        MemoryGarnishmentRequestStoreFindsWhatItSaved();
    }
}
