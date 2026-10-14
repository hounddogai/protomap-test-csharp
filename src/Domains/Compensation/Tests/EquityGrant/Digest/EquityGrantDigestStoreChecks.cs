namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantDigestStoreChecks
{
    public static void MemoryEquityGrantDigestStoreFindsWhatItSaved()
    {
        var store = EquityGrantDigestStore.NewMemoryEquityGrantDigestStore();
        var item = EquityGrantDigest.NewEquityGrantDigest("code-785", "label-856");
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
        MemoryEquityGrantDigestStoreFindsWhatItSaved();
    }
}
