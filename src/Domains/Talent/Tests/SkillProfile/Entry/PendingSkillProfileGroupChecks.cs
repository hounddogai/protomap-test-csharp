namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingSkillProfileGroupChecks
{
    public static void PendingSkillProfileGroupStartsInPlanned()
    {
        var item = PendingSkillProfileGroup.NewPendingSkillProfileGroup("code-161", "title-710");
        Check.Equal(item.Status, PendingSkillProfileGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSkillProfileGroupMovesToScheduled()
    {
        var item = PendingSkillProfileGroup.NewPendingSkillProfileGroup("code-161", "title-710");
        try
        {
            item.MoveTo(PendingSkillProfileGroupStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSkillProfileGroupStatus.Scheduled, "status");
    }

    public static void PendingSkillProfileGroupRejectsMovesFromCancelled()
    {
        var item = PendingSkillProfileGroup.NewPendingSkillProfileGroup("code-161", "title-710");
        item.Status = PendingSkillProfileGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingSkillProfileGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingSkillProfileGroupStartsInPlanned();
        PendingSkillProfileGroupMovesToScheduled();
        PendingSkillProfileGroupRejectsMovesFromCancelled();
    }
}
