namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientPayRunAgreementStoreChecks
{
    public static void MemoryLenientPayRunAgreementStoreFindsWhatItSaved()
    {
        var store = LenientPayRunAgreementStore.NewMemoryLenientPayRunAgreementStore();
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("code-598", "title-119");
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
        MemoryLenientPayRunAgreementStoreFindsWhatItSaved();
    }
}
