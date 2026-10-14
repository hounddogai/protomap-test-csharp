namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalAuditClaimStoreChecks
{
    public static void MemorySeasonalAuditClaimStoreFindsWhatItSaved()
    {
        var store = SeasonalAuditClaimStore.NewMemorySeasonalAuditClaimStore();
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("code-665", "label-755");
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
        MemorySeasonalAuditClaimStoreFindsWhatItSaved();
    }
}
