namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class PrimaryRetroPayAuditStoreChecks
{
    public static void MemoryPrimaryRetroPayAuditStoreFindsWhatItSaved()
    {
        var store = PrimaryRetroPayAuditStore.NewMemoryPrimaryRetroPayAuditStore();
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("code-326", "label-744");
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
        MemoryPrimaryRetroPayAuditStoreFindsWhatItSaved();
    }
}
