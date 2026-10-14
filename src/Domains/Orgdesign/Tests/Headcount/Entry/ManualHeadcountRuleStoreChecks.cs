namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualHeadcountRuleStoreChecks
{
    public static void MemoryManualHeadcountRuleStoreFindsWhatItSaved()
    {
        var store = ManualHeadcountRuleStore.NewMemoryManualHeadcountRuleStore();
        var item = ManualHeadcountRule.NewManualHeadcountRule("code-328", "name-997");
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
        MemoryManualHeadcountRuleStoreFindsWhatItSaved();
    }
}
