namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class ManualSkillProfileExceptionChecks
{
    public static void ManualSkillProfileExceptionStartsInNew()
    {
        var item = ManualSkillProfileException.NewManualSkillProfileException("code-841", "name-705");
        Check.Equal(item.Status, ManualSkillProfileExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSkillProfileExceptionMovesToRunning()
    {
        var item = ManualSkillProfileException.NewManualSkillProfileException("code-841", "name-705");
        try
        {
            item.MoveTo(ManualSkillProfileExceptionStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSkillProfileExceptionStatus.Running, "status");
    }

    public static void ManualSkillProfileExceptionRejectsMovesFromFailed()
    {
        var item = ManualSkillProfileException.NewManualSkillProfileException("code-841", "name-705");
        item.Status = ManualSkillProfileExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualSkillProfileExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualSkillProfileExceptionStartsInNew();
        ManualSkillProfileExceptionMovesToRunning();
        ManualSkillProfileExceptionRejectsMovesFromFailed();
    }
}
