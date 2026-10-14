namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class SeasonalSkillProfileCycleStoreChecks
{
    public static void MemorySeasonalSkillProfileCycleStoreFindsWhatItSaved()
    {
        var store = SeasonalSkillProfileCycleStore.NewMemorySeasonalSkillProfileCycleStore();
        var item = SeasonalSkillProfileCycle.NewSeasonalSkillProfileCycle("code-787", "label-12");
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
        MemorySeasonalSkillProfileCycleStoreFindsWhatItSaved();
    }
}
