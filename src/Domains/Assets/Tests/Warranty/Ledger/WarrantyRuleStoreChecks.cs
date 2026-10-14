namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WarrantyRuleStoreChecks
{
    public static void MemoryWarrantyRuleStoreFindsWhatItSaved()
    {
        var store = WarrantyRuleStore.NewMemoryWarrantyRuleStore();
        var item = WarrantyRule.NewWarrantyRule("code-431", "label-487");
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
        MemoryWarrantyRuleStoreFindsWhatItSaved();
    }
}
