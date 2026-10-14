namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileManifestStoreChecks
{
    public static void MemorySkillProfileManifestStoreFindsWhatItSaved()
    {
        var store = SkillProfileManifestStore.NewMemorySkillProfileManifestStore();
        var item = SkillProfileManifest.NewSkillProfileManifest("code-372", "label-72");
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
        MemorySkillProfileManifestStoreFindsWhatItSaved();
    }
}
