namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PositionRuleStoreChecks
{
    public static void MemoryPositionRuleStoreFindsWhatItSaved()
    {
        var store = PositionRuleStore.NewMemoryPositionRuleStore();
        var item = PositionRule.NewPositionRule("code-185", "label-752");
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
        MemoryPositionRuleStoreFindsWhatItSaved();
    }
}
