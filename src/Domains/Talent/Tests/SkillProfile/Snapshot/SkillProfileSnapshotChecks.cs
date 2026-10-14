namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileSnapshotChecks
{
    public static void SkillProfileSnapshotStartsInOpen()
    {
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("code-496", "label-781");
        Check.Equal(item.Status, SkillProfileSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileSnapshotMovesToReopened()
    {
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("code-496", "label-781");
        try
        {
            item.MoveTo(SkillProfileSnapshotStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileSnapshotStatus.Reopened, "status");
    }

    public static void SkillProfileSnapshotRejectsMovesFromArchived()
    {
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("code-496", "label-781");
        item.Status = SkillProfileSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(SkillProfileSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        SkillProfileSnapshotStartsInOpen();
        SkillProfileSnapshotMovesToReopened();
        SkillProfileSnapshotRejectsMovesFromArchived();
    }
}
