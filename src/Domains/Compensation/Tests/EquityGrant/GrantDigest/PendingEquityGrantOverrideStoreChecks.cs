namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PendingEquityGrantOverrideStoreChecks
{
    public static void MemoryPendingEquityGrantOverrideStoreFindsWhatItSaved()
    {
        var store = PendingEquityGrantOverrideStore.NewMemoryPendingEquityGrantOverrideStore();
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("code-522", "name-670");
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
        MemoryPendingEquityGrantOverrideStoreFindsWhatItSaved();
    }
}
