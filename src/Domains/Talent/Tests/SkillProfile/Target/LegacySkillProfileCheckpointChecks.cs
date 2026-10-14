namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacySkillProfileCheckpointChecks
{
    public static void LegacySkillProfileCheckpointStartsInPlanned()
    {
        var item = LegacySkillProfileCheckpoint.NewLegacySkillProfileCheckpoint("code-696", "name-942");
        Check.Equal(item.Status, LegacySkillProfileCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySkillProfileCheckpointMovesToScheduled()
    {
        var item = LegacySkillProfileCheckpoint.NewLegacySkillProfileCheckpoint("code-696", "name-942");
        try
        {
            item.MoveTo(LegacySkillProfileCheckpointStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySkillProfileCheckpointStatus.Scheduled, "status");
    }

    public static void LegacySkillProfileCheckpointRejectsMovesFromCancelled()
    {
        var item = LegacySkillProfileCheckpoint.NewLegacySkillProfileCheckpoint("code-696", "name-942");
        item.Status = LegacySkillProfileCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacySkillProfileCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacySkillProfileCheckpointStartsInPlanned();
        LegacySkillProfileCheckpointMovesToScheduled();
        LegacySkillProfileCheckpointRejectsMovesFromCancelled();
    }
}
