namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationAuditStoreChecks
{
    public static void MemoryNominationAuditStoreFindsWhatItSaved()
    {
        var store = NominationAuditStore.NewMemoryNominationAuditStore();
        var item = NominationAudit.NewNominationAudit("code-156", "name-536");
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
        MemoryNominationAuditStoreFindsWhatItSaved();
    }
}
