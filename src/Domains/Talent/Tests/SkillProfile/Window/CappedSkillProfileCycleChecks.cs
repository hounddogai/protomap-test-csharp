namespace Acme.Workforce.Talent.Tests.SkillProfile.Window;

using Acme.Workforce.Talent.SkillProfile.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedSkillProfileCycleChecks
{
    public static void CappedSkillProfileCycleStartsInProposed()
    {
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("code-101", "title-996");
        Check.Equal(item.Status, CappedSkillProfileCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSkillProfileCycleMovesToApproved()
    {
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("code-101", "title-996");
        try
        {
            item.MoveTo(CappedSkillProfileCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSkillProfileCycleStatus.Approved, "status");
    }

    public static void CappedSkillProfileCycleRejectsMovesFromWithdrawn()
    {
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("code-101", "title-996");
        item.Status = CappedSkillProfileCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedSkillProfileCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedSkillProfileCycleStartsInProposed();
        CappedSkillProfileCycleMovesToApproved();
        CappedSkillProfileCycleRejectsMovesFromWithdrawn();
    }
}
