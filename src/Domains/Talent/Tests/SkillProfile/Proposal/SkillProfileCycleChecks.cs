namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileCycleChecks
{
    public static void SkillProfileCycleStartsInPlanned()
    {
        var item = SkillProfileCycle.NewSkillProfileCycle("code-608", "name-23");
        Check.Equal(item.Status, SkillProfileCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileCycleMovesToScheduled()
    {
        var item = SkillProfileCycle.NewSkillProfileCycle("code-608", "name-23");
        try
        {
            item.MoveTo(SkillProfileCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileCycleStatus.Scheduled, "status");
    }

    public static void SkillProfileCycleRejectsMovesFromCancelled()
    {
        var item = SkillProfileCycle.NewSkillProfileCycle("code-608", "name-23");
        item.Status = SkillProfileCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileCycleStartsInPlanned();
        SkillProfileCycleMovesToScheduled();
        SkillProfileCycleRejectsMovesFromCancelled();
    }
}
