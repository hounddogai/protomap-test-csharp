namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultAuditTierStoreChecks
{
    public static void MemoryDefaultAuditTierStoreFindsWhatItSaved()
    {
        var store = DefaultAuditTierStore.NewMemoryDefaultAuditTierStore();
        var item = DefaultAuditTier.NewDefaultAuditTier("code-926", "label-709");
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
        MemoryDefaultAuditTierStoreFindsWhatItSaved();
    }
}
