namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class SkillProfileQuotaStoreChecks
{
    public static void MemorySkillProfileQuotaStoreFindsWhatItSaved()
    {
        var store = SkillProfileQuotaStore.NewMemorySkillProfileQuotaStore();
        var item = SkillProfileQuota.NewSkillProfileQuota("code-91", "label-572");
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
        MemorySkillProfileQuotaStoreFindsWhatItSaved();
    }
}
