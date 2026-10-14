namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class GlobalCareerPathRuleStoreChecks
{
    public static void MemoryGlobalCareerPathRuleStoreFindsWhatItSaved()
    {
        var store = GlobalCareerPathRuleStore.NewMemoryGlobalCareerPathRuleStore();
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("code-416", "label-424");
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
        MemoryGlobalCareerPathRuleStoreFindsWhatItSaved();
    }
}
