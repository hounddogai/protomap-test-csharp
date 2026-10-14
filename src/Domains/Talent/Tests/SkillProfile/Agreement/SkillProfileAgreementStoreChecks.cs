namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SkillProfileAgreementStoreChecks
{
    public static void MemorySkillProfileAgreementStoreFindsWhatItSaved()
    {
        var store = SkillProfileAgreementStore.NewMemorySkillProfileAgreementStore();
        var item = SkillProfileAgreement.NewSkillProfileAgreement("code-896", "title-278");
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
        MemorySkillProfileAgreementStoreFindsWhatItSaved();
    }
}
