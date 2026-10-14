namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PayRunAgreementStoreChecks
{
    public static void MemoryPayRunAgreementStoreFindsWhatItSaved()
    {
        var store = PayRunAgreementStore.NewMemoryPayRunAgreementStore();
        var item = PayRunAgreement.NewPayRunAgreement("code-816", "name-763");
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
        MemoryPayRunAgreementStoreFindsWhatItSaved();
    }
}
