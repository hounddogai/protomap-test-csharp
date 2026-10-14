namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingPayRunDigestStoreChecks
{
    public static void MemoryRollingPayRunDigestStoreFindsWhatItSaved()
    {
        var store = RollingPayRunDigestStore.NewMemoryRollingPayRunDigestStore();
        var item = RollingPayRunDigest.NewRollingPayRunDigest("code-606", "title-78");
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
        MemoryRollingPayRunDigestStoreFindsWhatItSaved();
    }
}
