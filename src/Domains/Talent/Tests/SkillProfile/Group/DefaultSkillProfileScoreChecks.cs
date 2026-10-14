namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultSkillProfileScoreChecks
{
    public static void DefaultSkillProfileScoreStartsInIdle()
    {
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("code-868", "label-883");
        Check.Equal(item.Status, DefaultSkillProfileScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSkillProfileScoreMovesToWarming()
    {
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("code-868", "label-883");
        try
        {
            item.MoveTo(DefaultSkillProfileScoreStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSkillProfileScoreStatus.Warming, "status");
    }

    public static void DefaultSkillProfileScoreRejectsMovesFromStopped()
    {
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("code-868", "label-883");
        item.Status = DefaultSkillProfileScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultSkillProfileScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScoreStartsInIdle();
        DefaultSkillProfileScoreMovesToWarming();
        DefaultSkillProfileScoreRejectsMovesFromStopped();
    }
}
