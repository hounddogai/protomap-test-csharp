namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopAgreementStoreChecks
{
    public static void MemoryLaptopAgreementStoreFindsWhatItSaved()
    {
        var store = LaptopAgreementStore.NewMemoryLaptopAgreementStore();
        var item = LaptopAgreement.NewLaptopAgreement("code-867", "label-920");
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
        MemoryLaptopAgreementStoreFindsWhatItSaved();
    }
}
