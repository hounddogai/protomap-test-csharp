namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyAgreementStoreChecks
{
    public static void MemoryWarrantyAgreementStoreFindsWhatItSaved()
    {
        var store = WarrantyAgreementStore.NewMemoryWarrantyAgreementStore();
        var item = WarrantyAgreement.NewWarrantyAgreement("code-917", "title-106");
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
        MemoryWarrantyAgreementStoreFindsWhatItSaved();
    }
}
