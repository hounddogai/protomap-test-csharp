namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileGroupStoreChecks
{
    public static void MemorySkillProfileGroupStoreFindsWhatItSaved()
    {
        var store = SkillProfileGroupStore.NewMemorySkillProfileGroupStore();
        var item = SkillProfileGroup.NewSkillProfileGroup("code-986", "name-326");
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
        MemorySkillProfileGroupStoreFindsWhatItSaved();
    }
}
