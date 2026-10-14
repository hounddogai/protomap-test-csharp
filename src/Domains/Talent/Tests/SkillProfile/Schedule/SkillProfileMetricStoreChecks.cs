namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SkillProfileMetricStoreChecks
{
    public static void MemorySkillProfileMetricStoreFindsWhatItSaved()
    {
        var store = SkillProfileMetricStore.NewMemorySkillProfileMetricStore();
        var item = SkillProfileMetric.NewSkillProfileMetric("code-371", "title-812");
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
        MemorySkillProfileMetricStoreFindsWhatItSaved();
    }
}
