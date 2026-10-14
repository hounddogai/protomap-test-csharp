namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SkillProfileNoticeViewChecks
{
    public static void SkillProfileNoticeViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileNotice.NewSkillProfileNotice("code-446", "title-602");
        var view = SkillProfileNoticeView.ToSkillProfileNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileNoticeView.ToSkillProfileNoticeViews(new List<SkillProfileNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileNoticeViewShowsTheStatusInCapitals();
    }
}
