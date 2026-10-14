namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorRuleStoreChecks
{
    public static void MemoryVisitorRuleStoreFindsWhatItSaved()
    {
        var store = VisitorRuleStore.NewMemoryVisitorRuleStore();
        var item = VisitorRule.NewVisitorRule("code-632", "title-328");
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
        MemoryVisitorRuleStoreFindsWhatItSaved();
    }
}
