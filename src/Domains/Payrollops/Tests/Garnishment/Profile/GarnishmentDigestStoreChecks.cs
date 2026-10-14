namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentDigestStoreChecks
{
    public static void MemoryGarnishmentDigestStoreFindsWhatItSaved()
    {
        var store = GarnishmentDigestStore.NewMemoryGarnishmentDigestStore();
        var item = GarnishmentDigest.NewGarnishmentDigest("code-833", "label-310");
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
        MemoryGarnishmentDigestStoreFindsWhatItSaved();
    }
}
