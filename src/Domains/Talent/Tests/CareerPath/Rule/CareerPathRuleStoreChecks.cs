namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathRuleStoreChecks
{
    public static void MemoryCareerPathRuleStoreFindsWhatItSaved()
    {
        var store = CareerPathRuleStore.NewMemoryCareerPathRuleStore();
        var item = CareerPathRule.NewCareerPathRule("code-145", "title-248");
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
        MemoryCareerPathRuleStoreFindsWhatItSaved();
    }
}
