namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScheduledSkillProfilePolicyStoreChecks
{
    public static void MemoryScheduledSkillProfilePolicyStoreFindsWhatItSaved()
    {
        var store = ScheduledSkillProfilePolicyStore.NewMemoryScheduledSkillProfilePolicyStore();
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("code-84", "label-310");
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
        MemoryScheduledSkillProfilePolicyStoreFindsWhatItSaved();
    }
}
