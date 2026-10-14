namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ReorgRuleStoreChecks
{
    public static void MemoryReorgRuleStoreFindsWhatItSaved()
    {
        var store = ReorgRuleStore.NewMemoryReorgRuleStore();
        var item = ReorgRule.NewReorgRule("code-199", "name-253");
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
        MemoryReorgRuleStoreFindsWhatItSaved();
    }
}
