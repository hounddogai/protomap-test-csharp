namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantAgreementStoreChecks
{
    public static void MemoryEquityGrantAgreementStoreFindsWhatItSaved()
    {
        var store = EquityGrantAgreementStore.NewMemoryEquityGrantAgreementStore();
        var item = EquityGrantAgreement.NewEquityGrantAgreement("code-471", "name-836");
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
        MemoryEquityGrantAgreementStoreFindsWhatItSaved();
    }
}
