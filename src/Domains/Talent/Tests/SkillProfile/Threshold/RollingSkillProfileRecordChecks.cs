namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingSkillProfileRecordChecks
{
    public static void RollingSkillProfileRecordStartsInRequested()
    {
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("code-256", "name-927");
        Check.Equal(item.Status, RollingSkillProfileRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSkillProfileRecordMovesToGranted()
    {
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("code-256", "name-927");
        try
        {
            item.MoveTo(RollingSkillProfileRecordStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSkillProfileRecordStatus.Granted, "status");
    }

    public static void RollingSkillProfileRecordRejectsMovesFromExpired()
    {
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("code-256", "name-927");
        item.Status = RollingSkillProfileRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingSkillProfileRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingSkillProfileRecordStartsInRequested();
        RollingSkillProfileRecordMovesToGranted();
        RollingSkillProfileRecordRejectsMovesFromExpired();
    }
}
