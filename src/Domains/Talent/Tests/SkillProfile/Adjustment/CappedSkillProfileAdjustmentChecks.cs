namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedSkillProfileAdjustmentChecks
{
    public static void CappedSkillProfileAdjustmentStartsInDraft()
    {
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("code-362", "name-317");
        Check.Equal(item.Status, CappedSkillProfileAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSkillProfileAdjustmentMovesToPending()
    {
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("code-362", "name-317");
        try
        {
            item.MoveTo(CappedSkillProfileAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSkillProfileAdjustmentStatus.Pending, "status");
    }

    public static void CappedSkillProfileAdjustmentRejectsMovesFromClosed()
    {
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("code-362", "name-317");
        item.Status = CappedSkillProfileAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedSkillProfileAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedSkillProfileAdjustmentStartsInDraft();
        CappedSkillProfileAdjustmentMovesToPending();
        CappedSkillProfileAdjustmentRejectsMovesFromClosed();
    }
}
