namespace Acme.Workforce.Talent.Tests.SkillProfile.Window;

using Acme.Workforce.Talent.SkillProfile.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedSkillProfileCycleStoreChecks
{
    public static void MemoryCappedSkillProfileCycleStoreFindsWhatItSaved()
    {
        var store = CappedSkillProfileCycleStore.NewMemoryCappedSkillProfileCycleStore();
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("code-101", "title-996");
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
        MemoryCappedSkillProfileCycleStoreFindsWhatItSaved();
    }
}
