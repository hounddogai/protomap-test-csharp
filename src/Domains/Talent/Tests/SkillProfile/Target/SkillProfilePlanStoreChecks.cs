namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfilePlanStoreChecks
{
    public static void MemorySkillProfilePlanStoreFindsWhatItSaved()
    {
        var store = SkillProfilePlanStore.NewMemorySkillProfilePlanStore();
        var item = SkillProfilePlan.NewSkillProfilePlan("code-760", "label-52");
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
        MemorySkillProfilePlanStoreFindsWhatItSaved();
    }
}
