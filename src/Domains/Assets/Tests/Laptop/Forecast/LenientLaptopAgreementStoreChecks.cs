namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LenientLaptopAgreementStoreChecks
{
    public static void MemoryLenientLaptopAgreementStoreFindsWhatItSaved()
    {
        var store = LenientLaptopAgreementStore.NewMemoryLenientLaptopAgreementStore();
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("code-740", "name-498");
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
        MemoryLenientLaptopAgreementStoreFindsWhatItSaved();
    }
}
