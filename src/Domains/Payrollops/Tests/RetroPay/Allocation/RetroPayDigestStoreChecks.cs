namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RetroPayDigestStoreChecks
{
    public static void MemoryRetroPayDigestStoreFindsWhatItSaved()
    {
        var store = RetroPayDigestStore.NewMemoryRetroPayDigestStore();
        var item = RetroPayDigest.NewRetroPayDigest("code-97", "title-609");
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
        MemoryRetroPayDigestStoreFindsWhatItSaved();
    }
}
