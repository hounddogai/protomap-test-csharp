namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SkillProfileThresholdStoreChecks
{
    public static void MemorySkillProfileThresholdStoreFindsWhatItSaved()
    {
        var store = SkillProfileThresholdStore.NewMemorySkillProfileThresholdStore();
        var item = SkillProfileThreshold.NewSkillProfileThreshold("code-314", "label-671");
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
        MemorySkillProfileThresholdStoreFindsWhatItSaved();
    }
}
