namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedSkillProfileBudgetStoreChecks
{
    public static void MemoryWeightedSkillProfileBudgetStoreFindsWhatItSaved()
    {
        var store = WeightedSkillProfileBudgetStore.NewMemoryWeightedSkillProfileBudgetStore();
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("code-469", "title-584");
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
        MemoryWeightedSkillProfileBudgetStoreFindsWhatItSaved();
    }
}
