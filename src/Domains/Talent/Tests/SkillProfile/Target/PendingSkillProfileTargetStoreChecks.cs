namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingSkillProfileTargetStoreChecks
{
    public static void MemoryPendingSkillProfileTargetStoreFindsWhatItSaved()
    {
        var store = PendingSkillProfileTargetStore.NewMemoryPendingSkillProfileTargetStore();
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("code-46", "name-390");
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
        MemoryPendingSkillProfileTargetStoreFindsWhatItSaved();
    }
}
