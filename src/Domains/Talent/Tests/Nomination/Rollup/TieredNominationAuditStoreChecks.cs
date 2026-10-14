namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredNominationAuditStoreChecks
{
    public static void MemoryTieredNominationAuditStoreFindsWhatItSaved()
    {
        var store = TieredNominationAuditStore.NewMemoryTieredNominationAuditStore();
        var item = TieredNominationAudit.NewTieredNominationAudit("code-874", "title-997");
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
        MemoryTieredNominationAuditStoreFindsWhatItSaved();
    }
}
