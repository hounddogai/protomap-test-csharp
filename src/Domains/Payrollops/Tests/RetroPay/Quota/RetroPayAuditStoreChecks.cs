namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAuditStoreChecks
{
    public static void MemoryRetroPayAuditStoreFindsWhatItSaved()
    {
        var store = RetroPayAuditStore.NewMemoryRetroPayAuditStore();
        var item = RetroPayAudit.NewRetroPayAudit("code-500", "name-39");
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
        MemoryRetroPayAuditStoreFindsWhatItSaved();
    }
}
