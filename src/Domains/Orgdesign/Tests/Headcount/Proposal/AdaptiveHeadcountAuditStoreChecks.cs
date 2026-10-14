namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AdaptiveHeadcountAuditStoreChecks
{
    public static void MemoryAdaptiveHeadcountAuditStoreFindsWhatItSaved()
    {
        var store = AdaptiveHeadcountAuditStore.NewMemoryAdaptiveHeadcountAuditStore();
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("code-192", "label-847");
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
        MemoryAdaptiveHeadcountAuditStoreFindsWhatItSaved();
    }
}
