namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SkillProfileBundleStoreChecks
{
    public static void MemorySkillProfileBundleStoreFindsWhatItSaved()
    {
        var store = SkillProfileBundleStore.NewMemorySkillProfileBundleStore();
        var item = SkillProfileBundle.NewSkillProfileBundle("code-230", "label-715");
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
        MemorySkillProfileBundleStoreFindsWhatItSaved();
    }
}
