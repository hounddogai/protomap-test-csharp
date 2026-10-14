namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SkillProfileNoticeStoreChecks
{
    public static void MemorySkillProfileNoticeStoreFindsWhatItSaved()
    {
        var store = SkillProfileNoticeStore.NewMemorySkillProfileNoticeStore();
        var item = SkillProfileNotice.NewSkillProfileNotice("code-446", "title-602");
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
        MemorySkillProfileNoticeStoreFindsWhatItSaved();
    }
}
