namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SkillProfileNoticeChecks
{
    public static void SkillProfileNoticeStartsInCreated()
    {
        var item = SkillProfileNotice.NewSkillProfileNotice("code-446", "title-602");
        Check.Equal(item.Status, SkillProfileNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileNoticeMovesToValidated()
    {
        var item = SkillProfileNotice.NewSkillProfileNotice("code-446", "title-602");
        try
        {
            item.MoveTo(SkillProfileNoticeStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileNoticeStatus.Validated, "status");
    }

    public static void SkillProfileNoticeRejectsMovesFromRetired()
    {
        var item = SkillProfileNotice.NewSkillProfileNotice("code-446", "title-602");
        item.Status = SkillProfileNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(SkillProfileNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        SkillProfileNoticeStartsInCreated();
        SkillProfileNoticeMovesToValidated();
        SkillProfileNoticeRejectsMovesFromRetired();
    }
}
