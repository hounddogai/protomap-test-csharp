namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultSkillProfileScheduleStoreChecks
{
    public static void MemoryDefaultSkillProfileScheduleStoreFindsWhatItSaved()
    {
        var store = DefaultSkillProfileScheduleStore.NewMemoryDefaultSkillProfileScheduleStore();
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("code-114", "title-957");
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
        MemoryDefaultSkillProfileScheduleStoreFindsWhatItSaved();
    }
}
