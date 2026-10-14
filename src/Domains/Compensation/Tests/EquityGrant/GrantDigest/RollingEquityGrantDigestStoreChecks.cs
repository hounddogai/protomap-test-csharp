namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class RollingEquityGrantDigestStoreChecks
{
    public static void MemoryRollingEquityGrantDigestStoreFindsWhatItSaved()
    {
        var store = RollingEquityGrantDigestStore.NewMemoryRollingEquityGrantDigestStore();
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("code-254", "label-418");
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
        MemoryRollingEquityGrantDigestStoreFindsWhatItSaved();
    }
}
