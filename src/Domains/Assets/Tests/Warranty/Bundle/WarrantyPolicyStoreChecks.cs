namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyPolicyStoreChecks
{
    public static void MemoryWarrantyPolicyStoreFindsWhatItSaved()
    {
        var store = WarrantyPolicyStore.NewMemoryWarrantyPolicyStore();
        var item = WarrantyPolicy.NewWarrantyPolicy("code-199", "label-910");
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
        MemoryWarrantyPolicyStoreFindsWhatItSaved();
    }
}
