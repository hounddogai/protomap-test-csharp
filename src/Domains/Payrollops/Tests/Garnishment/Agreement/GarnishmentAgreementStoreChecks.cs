namespace Acme.Workforce.Payrollops.Tests.Garnishment.Agreement;

using Acme.Workforce.Payrollops.Garnishment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GarnishmentAgreementStoreChecks
{
    public static void MemoryGarnishmentAgreementStoreFindsWhatItSaved()
    {
        var store = GarnishmentAgreementStore.NewMemoryGarnishmentAgreementStore();
        var item = GarnishmentAgreement.NewGarnishmentAgreement("code-840", "title-387");
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
        MemoryGarnishmentAgreementStoreFindsWhatItSaved();
    }
}
