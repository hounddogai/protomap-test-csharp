namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAgreementStoreChecks
{
    public static void MemoryLegacyPayRunAgreementStoreFindsWhatItSaved()
    {
        var store = LegacyPayRunAgreementStore.NewMemoryLegacyPayRunAgreementStore();
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("code-833", "title-261");
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
        MemoryLegacyPayRunAgreementStoreFindsWhatItSaved();
    }
}
