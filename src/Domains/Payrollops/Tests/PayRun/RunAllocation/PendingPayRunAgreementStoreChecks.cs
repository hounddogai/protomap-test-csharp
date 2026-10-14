namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class PendingPayRunAgreementStoreChecks
{
    public static void MemoryPendingPayRunAgreementStoreFindsWhatItSaved()
    {
        var store = PendingPayRunAgreementStore.NewMemoryPendingPayRunAgreementStore();
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("code-509", "title-140");
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
        MemoryPendingPayRunAgreementStoreFindsWhatItSaved();
    }
}
