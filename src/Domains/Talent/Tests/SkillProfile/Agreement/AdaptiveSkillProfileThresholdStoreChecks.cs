namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileThresholdStoreChecks
{
    public static void MemoryAdaptiveSkillProfileThresholdStoreFindsWhatItSaved()
    {
        var store = AdaptiveSkillProfileThresholdStore.NewMemoryAdaptiveSkillProfileThresholdStore();
        var item = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold("code-828", "title-885");
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
        MemoryAdaptiveSkillProfileThresholdStoreFindsWhatItSaved();
    }
}
