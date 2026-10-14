namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultVisitorRuleStoreChecks
{
    public static void MemoryDefaultVisitorRuleStoreFindsWhatItSaved()
    {
        var store = DefaultVisitorRuleStore.NewMemoryDefaultVisitorRuleStore();
        var item = DefaultVisitorRule.NewDefaultVisitorRule("code-619", "label-151");
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
        MemoryDefaultVisitorRuleStoreFindsWhatItSaved();
    }
}
