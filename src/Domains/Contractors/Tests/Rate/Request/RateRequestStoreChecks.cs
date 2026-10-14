namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateRequestStoreChecks
{
    public static void MemoryRateRequestStoreFindsWhatItSaved()
    {
        var store = RateRequestStore.NewMemoryRateRequestStore();
        var item = RateRequest.NewRateRequest("code-549", "label-933");
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
        MemoryRateRequestStoreFindsWhatItSaved();
    }
}
