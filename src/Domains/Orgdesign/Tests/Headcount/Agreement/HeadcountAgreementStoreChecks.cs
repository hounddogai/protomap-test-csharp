namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountAgreementStoreChecks
{
    public static void MemoryHeadcountAgreementStoreFindsWhatItSaved()
    {
        var store = HeadcountAgreementStore.NewMemoryHeadcountAgreementStore();
        var item = HeadcountAgreement.NewHeadcountAgreement("code-372", "label-801");
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
        MemoryHeadcountAgreementStoreFindsWhatItSaved();
    }
}
