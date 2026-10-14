namespace Acme.Workforce.Talent.Tests.SkillProfile.Score;

using Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SkillProfileExceptionChecks
{
    public static void SkillProfileExceptionStartsInIdle()
    {
        var item = SkillProfileException.NewSkillProfileException("code-904", "title-898");
        Check.Equal(item.Status, SkillProfileExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileExceptionMovesToReady()
    {
        var item = SkillProfileException.NewSkillProfileException("code-904", "title-898");
        try
        {
            item.MoveTo(SkillProfileExceptionStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileExceptionStatus.Ready, "status");
    }

    public static void SkillProfileExceptionRejectsMovesFromStopped()
    {
        var item = SkillProfileException.NewSkillProfileException("code-904", "title-898");
        item.Status = SkillProfileExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(SkillProfileExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SkillProfileExceptionStartsInIdle();
        SkillProfileExceptionMovesToReady();
        SkillProfileExceptionRejectsMovesFromStopped();
    }
}
