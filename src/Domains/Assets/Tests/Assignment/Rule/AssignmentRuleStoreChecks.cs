namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentRuleStoreChecks
{
    public static void MemoryAssignmentRuleStoreFindsWhatItSaved()
    {
        var store = AssignmentRuleStore.NewMemoryAssignmentRuleStore();
        var item = AssignmentRule.NewAssignmentRule("code-37", "title-585");
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
        MemoryAssignmentRuleStoreFindsWhatItSaved();
    }
}
