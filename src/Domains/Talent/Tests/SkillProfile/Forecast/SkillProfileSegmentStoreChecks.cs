namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileSegmentStoreChecks
{
    public static void MemorySkillProfileSegmentStoreFindsWhatItSaved()
    {
        var store = SkillProfileSegmentStore.NewMemorySkillProfileSegmentStore();
        var item = SkillProfileSegment.NewSkillProfileSegment("code-501", "label-779");
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
        MemorySkillProfileSegmentStoreFindsWhatItSaved();
    }
}
