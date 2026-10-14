namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopRuleStoreChecks
{
    public static void MemoryLaptopRuleStoreFindsWhatItSaved()
    {
        var store = LaptopRuleStore.NewMemoryLaptopRuleStore();
        var item = LaptopRule.NewLaptopRule("code-427", "title-649");
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
        MemoryLaptopRuleStoreFindsWhatItSaved();
    }
}
