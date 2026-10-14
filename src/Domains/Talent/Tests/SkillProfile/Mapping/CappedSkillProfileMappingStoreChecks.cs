namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class CappedSkillProfileMappingStoreChecks
{
    public static void MemoryCappedSkillProfileMappingStoreFindsWhatItSaved()
    {
        var store = CappedSkillProfileMappingStore.NewMemoryCappedSkillProfileMappingStore();
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("code-284", "title-401");
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
        MemoryCappedSkillProfileMappingStoreFindsWhatItSaved();
    }
}
