namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileBudgetStoreChecks
{
    public static void MemorySkillProfileBudgetStoreFindsWhatItSaved()
    {
        var store = SkillProfileBudgetStore.NewMemorySkillProfileBudgetStore();
        var item = SkillProfileBudget.NewSkillProfileBudget("code-739", "title-144");
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
        MemorySkillProfileBudgetStoreFindsWhatItSaved();
    }
}
